using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

public sealed class GetFabricMovementsHandler(IWarehouseDbContext db)
    : IQueryHandler<GetFabricMovementsQuery, Result<FabricMovementPageResponse>>
{
    public const int PageSize = 50;

    public async ValueTask<Result<FabricMovementPageResponse>> Handle(GetFabricMovementsQuery query,
        CancellationToken cancellationToken)
    {
        var shelf = await FabricMovementLedger.ShelfAsync(db, cancellationToken);
        var matching = FabricMovementLedger.Filter(db.FabricMovements.AsNoTracking(), query, shelf);
        var groups = await FabricMovementLedger.GroupsAsync(matching, cancellationToken);
        var total = FabricMovementLedger.Count(groups, query.Kind);
        var rows = await matching.Where(FabricMovementLedger.Of(query.Kind)).Include(x => x.Stock)
            .OrderByDescending(x => x.OperationDate).ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * PageSize).Take(PageSize)
            .ToListAsync(cancellationToken);

        var products = await ProductsAsync(rows, cancellationToken);
        var pairs = await FabricMovementLedger.PairBalancesAsync(db, shelf, rows, cancellationToken);
        var page = rows
            .Select(x => Map(x, query.Actor, x.IssueLineId is { } line ? products.GetValueOrDefault(line) : null,
                pairs.GetValueOrDefault(x.Id.Value)))
            .ToList();
        return Result.Success(new FabricMovementPageResponse(page, query.Page, total > query.Page * PageSize,
            StockList.Permissions(query.Actor), total, FabricMovementLedger.Counts(groups)));
    }

    /// <summary>
    /// «На 1 виріб» (TZ §6.3): the products cut for the issue each movement belongs to, by its issue line; issues made
    /// before issues by actual cutting have no count.
    /// </summary>
    private async Task<Dictionary<int, int>> ProductsAsync(IEnumerable<FabricMovement> movements,
        CancellationToken cancellationToken)
    {
        var lines = movements.Select(x => x.IssueLineId).OfType<int>().Distinct().ToArray();
        if (lines.Length == 0) return [];
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines)
            .Where(x => x.IssuedQuantity != null && x.Lines.Any(l => lines.Contains(l.Id)))
            .ToListAsync(cancellationToken);
        return issues.SelectMany(x => x.Lines.Select(l => (l.Id, Products: x.IssuedQuantity!.Value)))
            .Where(x => lines.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Products);
    }

    /// <summary>Metres of the movement per product, to thousandths; null without a products count.</summary>
    public static decimal? PerProduct(decimal delta, int? products) =>
        products is > 0 ? decimal.Round(Math.Abs(delta) / products.Value, 3, MidpointRounding.AwayFromZero) : null;

    public static FabricMovementRowResponse Map(FabricMovement x, WarehouseActor actor, int? products = null,
        decimal? pairMetersAfter = null) => new(x.Id.Value, x.StockId.Value,
        x.Kind.ToString(), x.OperationDate, x.CreatedAt, x.Stock.FabricCode, FabricAliases.Canonical(x.Stock.FabricCode),
        x.Stock.FabricName, x.Stock.Color, x.SupplierName, x.Document, x.OrderId?.Value, x.ItemId?.Value, x.OrderNumber, x.ModelName,
        x.Destination, x.Delta, x.MetersAfter, x.Note, x.CreatedByName, x.IsReversed, x.ReversesMovementId?.Value,
        actor.CanMoveStock && ReverseFabricMovementHandler.CanReverse(x), x.IsVoided, x.Revision,
        actor.CanMoveStock && x.CanRevise, PerProduct(x.Delta, products), pairMetersAfter);
}
