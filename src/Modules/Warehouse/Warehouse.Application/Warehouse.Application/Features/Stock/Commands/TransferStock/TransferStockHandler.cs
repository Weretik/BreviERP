using System.Security.Cryptography;
using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

public sealed class TransferStockHandler(IWarehouseDbContext db, IWarehouseReferenceReader reference,
    IWarehouseProductionReader production, IWarehouseStaffReader staff, TimeProvider clock)
    : ICommandHandler<TransferStockCommand, Result<StockTransferResult>>
{
    private const string Transfer = "Перенесення зі старого «Цеху»";

    public async ValueTask<Result<StockTransferResult>> Handle(TransferStockCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanImport) return Result<StockTransferResult>.Forbidden(StockMessages.CannotImport);
        var input = command.Input;
        var hash = Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(input with
        {
            Snapshot = null
        })));
        var source = V133Source.Read(input);
        var current = await CurrentAsync(cancellationToken);
        var last = await db.StockTransfers.AsNoTracking().OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var own = current.OwnLines + current.OwnMovements + current.OwnIssues > 0;

        StockTransferResult Result(bool applied, bool unchanged, StockTransferTarget? target) => new(applied, unchanged,
            hash, source.Summary(), target, current, own, source.Errors, source.Notes, last?.CreatedAt);

        if (source.Errors.Count > 0) return Result(false, false, null);
        var targets = await TargetsAsync(source, cancellationToken);
        if (source.Errors.Count > 0) return Result(false, false, null);
        if (last?.Hash == hash && !own && current.Lines > 0) return Result(false, true, null);

        var apply = !command.DryRun && (!own || command.ConfirmReplace);
        var fabrics = (await reference.FabricsAsync(cancellationToken)).Where(x => x.Code != null)
            .GroupBy(x => FabricAliases.Normalize(x.Code!)).ToDictionary(g => g.Key, g => g.First());
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await ClearAsync(cancellationToken);
        var writer = new V133Writer(source, targets, fabrics, NameAsync, clock.GetUtcNow());
        var target = await writer.WriteAsync(db, cancellationToken);
        if (target is null || source.Errors.Count > 0) return Result(false, false, null);

        db.StockTransfers.Add(new StockTransfer(hash, input.Snapshot, target.Lines, target.Movements, target.Issues,
            target.Meters, await ActorAsync(command.Actor, cancellationToken), clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        if (apply) await transaction.CommitAsync(cancellationToken);
        return Result(apply, false, target);
    }

    /// <summary>Old item id → its Brevi order item; null for an item of an order deleted in the old «Цех».</summary>
    private async Task<Dictionary<string, V133Target?>> TargetsAsync(V133Source source,
        CancellationToken cancellationToken)
    {
        var used = source.Batches.Values.Select(x => x.ItemId)
            .Concat(source.Movements.Select(x => x.Row.ItemId).OfType<string>())
            .Concat(source.Input.FabricReservations.Select(x => x.ItemId))
            .Where(x => x.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        var orderIds = used.Select(x => source.Items[x].Item.OrderId).Distinct(StringComparer.Ordinal).ToArray();
        var brevi = (await production.LegacyOrdersAsync(orderIds, cancellationToken))
            .ToDictionary(x => x.LegacyId, StringComparer.Ordinal);
        var result = new Dictionary<string, V133Target?>(StringComparer.Ordinal);
        foreach (var missing in orderIds.Where(x => !brevi.ContainsKey(x)))
        {
            var order = source.Orders[missing];
            if (order.DeletedAt is null)
                source.Error("production_orders", missing,
                    $"Завдання №{order.Seq} немає в Brevi: спершу перенесіть завдання цеху («Виробничі завдання → Імпорт»).");
            else
                source.Note("production_orders", missing,
                    $"Завдання №{order.Seq} видалене у старому «Цеху»: його видачі перенесено як рух без завдання.");
        }

        foreach (var id in used)
        {
            var (item, position) = source.Items[id];
            if (!brevi.TryGetValue(item.OrderId, out var order))
            {
                result[id] = null;
                continue;
            }

            var match = order.Items.FirstOrDefault(x => x.Position == position);
            var code = V133Source.Text(item.ModelArticle)?.ToUpperInvariant();
            if (match is null)
                source.Error("production_items", id, $"У завданні №{order.Number} Brevi немає позиції {position}.");
            else if (code is not null && match.ModelCode is not null ? code != match.ModelCode
                     : !string.Equals(V133Source.Text(item.ModelName), match.ModelName, StringComparison.OrdinalIgnoreCase))
                source.Error("production_items", id,
                    $"Позиція {position} завдання №{order.Number}: у старому «Цеху» {code ?? item.ModelName}, у Brevi {match.ModelCode ?? match.ModelName}.");
            else
                result[id] = new V133Target(new FabricMovementTarget(order.OrderId, match.ItemId, order.Number,
                    match.ModelName), match.Quantity);
        }

        return result;
    }

    private async Task<StockTransferCurrent> CurrentAsync(CancellationToken cancellationToken) => new(
        await db.FabricStocks.CountAsync(cancellationToken),
        await db.FabricMovements.CountAsync(cancellationToken),
        await db.FabricIssues.CountAsync(cancellationToken),
        await db.FabricItemPlans.CountAsync(cancellationToken),
        await db.FabricStocks.CountAsync(x => x.LegacyId == null, cancellationToken),
        await db.FabricMovements.CountAsync(x => x.LegacyId == null, cancellationToken),
        await db.FabricIssues.CountAsync(x => x.LegacyId == null, cancellationToken));

    /// <summary>Empties the stock: revisions, movements, issues with their lines, plans, lines and operations.</summary>
    private async Task ClearAsync(CancellationToken cancellationToken)
    {
        await db.FabricMovementRevisions.ExecuteDeleteAsync(cancellationToken);
        await db.FabricMovements.Where(x => x.ReversesMovementId != null)
            .ExecuteUpdateAsync(x => x.SetProperty(m => m.ReversesMovementId, (int?)null), cancellationToken);
        await db.FabricMovements.ExecuteDeleteAsync(cancellationToken);
        await db.FabricIssues.ExecuteDeleteAsync(cancellationToken);
        db.FabricItemPlans.RemoveRange(await db.FabricItemPlans.ToListAsync(cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
        await db.FabricStocks.ExecuteDeleteAsync(cancellationToken);
        await db.StockOperations.ExecuteDeleteAsync(cancellationToken);
    }

    private readonly Dictionary<string, string> _names = new(StringComparer.OrdinalIgnoreCase);

    private async Task<string> NameAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email)) return Transfer;
        if (_names.TryGetValue(email, out var name)) return name;
        name = (await staff.FindByEmailAsync(email.Trim(), cancellationToken))?.Name ?? Transfer;
        _names[email] = name;
        return name;
    }

    private async Task<string> ActorAsync(WarehouseActor actor, CancellationToken cancellationToken) =>
        (await staff.FindAsync(actor.UserId, cancellationToken))?.Name ?? Transfer;
}
