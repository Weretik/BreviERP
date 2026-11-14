using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Domain.Orders.Enums;
using Production.Domain.Orders.Rules;

namespace Production.Application.Features.Orders.Commands.ImportProductionOrders;

public sealed class ImportProductionOrdersHandler(IProductionDbContext db, IProductionStaffReader staff,
    TimeProvider clock) : ICommandHandler<ImportProductionOrdersCommand, Result<ProductionOrderImportResult>>
{
    public async ValueTask<Result<ProductionOrderImportResult>> Handle(ImportProductionOrdersCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.IsAdmin) return Result<ProductionOrderImportResult>.Forbidden(OrderMessages.CannotImport);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderImportResult>.Forbidden(OrderMessages.ActorUnknown);

        var moved = (await db.Orders.AsNoTracking().Where(x => x.LegacyId != null).Select(x => x.LegacyId!)
            .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
        var taken = (await db.Orders.AsNoTracking().Select(x => x.Number).ToListAsync(cancellationToken)).ToHashSet();
        var norms = await OrderNorms.ReadAsync(db,
            command.Rows.SelectMany(x => x.Items).Select(x => Code(x.ModelCode)), cancellationToken);
        var managers = new Dictionary<string, ProductionStaffMember?>(StringComparer.OrdinalIgnoreCase);
        var now = clock.GetUtcNow();
        var lines = new List<ProductionOrderImportLine>(command.Rows.Count);
        var orders = new List<ProductionOrder>();

        foreach (var row in command.Rows)
        {
            if (moved.Contains(row.LegacyId.Trim()))
            {
                lines.Add(new(row.Number, row.ClientName, row.Items.Count, "unchanged", null));
                continue;
            }

            if (!taken.Add(row.Number))
            {
                lines.Add(new(row.Number, row.ClientName, row.Items.Count, "error", "Номер завдання вже зайнятий."));
                continue;
            }

            ProductionStaffMember? manager = null;
            if (!string.IsNullOrWhiteSpace(row.ManagerEmail))
            {
                var email = row.ManagerEmail.Trim();
                if (!managers.TryGetValue(email, out manager))
                    managers[email] = manager = await staff.FindByEmailAsync(email, cancellationToken);
            }

            var items = row.Items.Select((item, index) =>
            {
                var details = item.Details.ToDetails();
                var code = Code(item.ModelCode);
                var colors = 1 + details.AdditionalColors.Count(x => !string.IsNullOrWhiteSpace(x));
                return ProductionItem.Create(index + 1,
                    new ItemSource(null, null, null, null, code, item.ModelName, null, item.FabricCode, item.FabricName,
                        item.SewingRate, null),
                    details, FabricPlan.Meters(code is null ? null : norms.GetValueOrDefault(code), colors, details.Quantity));
            }).ToArray();
            var created = items.FirstOrDefault(x => x.IsFailure)?.Error is { } itemError
                ? DomainResult<ProductionOrder>.Failure(itemError)
                : ProductionOrder.Create(row.Number,
                    new OrderOrigin(null, row.QuoteNumber, null, manager?.UserId, row.ManagerName, null, row.LegacyId),
                    new OrderHeader(row.ClientName, row.OrderDate, row.Comment), null, items.Select(x => x.Value),
                    actorName, now);
            if (created.IsFailure)
            {
                lines.Add(new(row.Number, row.ClientName, row.Items.Count, "error", created.Error!.Message));
                continue;
            }

            var order = created.Value;
            order.SetTransferredStatus(Enum.Parse<ProductionOrderStatus>(row.Status, ignoreCase: true));
            order.Record("Перенесено", "Зі старого «Цеху»", actorName, now);
            orders.Add(order);
            lines.Add(new(row.Number, row.ClientName, row.Items.Count, "created", null));
        }

        var errors = lines.Count(x => x.Action == "error");
        var apply = !command.DryRun && errors == 0;
        if (apply)
        {
            db.Orders.AddRange(orders);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ProductionOrderImportResult(!apply, lines.Count(x => x.Action == "created"),
            lines.Count(x => x.Action == "unchanged"), errors, lines));
    }

    private static string? Code(string? modelCode) =>
        string.IsNullOrWhiteSpace(modelCode) ? null : ModelFabricNorm.NormalizeCode(modelCode);
}
