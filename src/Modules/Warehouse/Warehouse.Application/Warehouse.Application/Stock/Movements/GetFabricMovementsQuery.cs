using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Movements;

/// <summary>Which movements «Рух тканини» shows.</summary>
public enum FabricMovementFilter
{
    All = 0,

    /// <summary>«Прихід»: receipts.</summary>
    Receipt = 1,

    /// <summary>«Витрата»: issues to orders and without an order.</summary>
    Issue = 2,

    /// <summary>«Внесені початкові залишки»: openings not taken back.</summary>
    Opening = 3
}

/// <summary>
/// «Рух тканини»: 50 movements per page, newest operation date first (then record time and id). Optional filters: a
/// fabric (any code of its group), its colour and an order item.
/// </summary>
public sealed record GetFabricMovementsQuery(
    WarehouseActor Actor,
    int Page,
    FabricMovementFilter Kind,
    string? FabricCode,
    string? Color,
    int? ItemId) : IQuery<Result<FabricMovementPageResponse>>;

public sealed class GetFabricMovementsQueryValidator : AbstractValidator<GetFabricMovementsQuery>
{
    public GetFabricMovementsQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.FabricCode).MaximumLength(FabricStock.CodeMaxLength);
        RuleFor(x => x.Color).MaximumLength(StockColor.MaxLength);
        RuleFor(x => x.ItemId).GreaterThan(0).When(x => x.ItemId is not null);
    }
}

public sealed class GetFabricMovementsQueryHandler(IWarehouseDbContext db)
    : IQueryHandler<GetFabricMovementsQuery, Result<FabricMovementPageResponse>>
{
    public const int PageSize = 50;

    public async ValueTask<Result<FabricMovementPageResponse>> Handle(GetFabricMovementsQuery query,
        CancellationToken cancellationToken)
    {
        var movements = db.FabricMovements.AsNoTracking().Include(x => x.Stock).AsQueryable();
        movements = query.Kind switch
        {
            FabricMovementFilter.Receipt => movements.Where(x => x.Kind == FabricMovementKind.Receipt),
            FabricMovementFilter.Issue => movements.Where(x => x.Kind == FabricMovementKind.Issue),
            FabricMovementFilter.Opening => movements.Where(x => x.Kind == FabricMovementKind.Opening && !x.IsReversed && !x.IsVoided),
            _ => movements
        };
        if (!string.IsNullOrWhiteSpace(query.FabricCode))
        {
            var codes = FabricAliases.Members(query.FabricCode).ToArray();
            movements = movements.Where(x => codes.Contains(x.Stock.FabricCode));
        }

        if (query.ItemId is { } itemId) movements = movements.Where(x => x.ItemId == itemId);

        // The ledger is a few thousand rows; colours typed before the NFC key are matched like on «Залишки».
        var key = string.IsNullOrWhiteSpace(query.Color) ? null : StockColor.Key(query.Color);
        var rows = (await movements.ToListAsync(cancellationToken))
            .Where(x => key is null || StockColor.Key(x.Stock.Color) == key)
            .OrderByDescending(x => x.OperationDate).ThenByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * PageSize).Take(PageSize + 1).ToList();

        var products = await ProductsAsync(rows.Take(PageSize), cancellationToken);
        var page = rows.Take(PageSize)
            .Select(x => Map(x, query.Actor, x.IssueLineId is { } line ? products.GetValueOrDefault(line) : null))
            .ToList();
        return Result.Success(new FabricMovementPageResponse(page, query.Page, rows.Count > PageSize,
            StockList.Permissions(query.Actor)));
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

    public static FabricMovementRowResponse Map(FabricMovement x, WarehouseActor actor, int? products = null) => new(x.Id, x.StockId,
        x.Kind.ToString(), x.OperationDate, x.CreatedAt, x.Stock.FabricCode, FabricAliases.Canonical(x.Stock.FabricCode),
        x.Stock.FabricName, x.Stock.Color, x.SupplierName, x.Document, x.OrderId, x.ItemId, x.OrderNumber, x.ModelName,
        x.Destination, x.Delta, x.MetersAfter, x.Note, x.CreatedByName, x.IsReversed, x.ReversesMovementId,
        actor.CanMoveStock && ReverseFabricMovementCommandHandler.CanReverse(x), x.IsVoided, x.Revision,
        actor.CanMoveStock && x.CanRevise, PerProduct(x.Delta, products));
}
