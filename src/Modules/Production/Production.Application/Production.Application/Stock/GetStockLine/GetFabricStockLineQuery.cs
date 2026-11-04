using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Stock;

namespace Production.Application.Stock.GetStockLine;

/// <summary>Card of a stock line: the balance, every movement newest first and the items holding it.</summary>
public sealed record GetFabricStockLineQuery(ProductionActor Actor, int StockId) : IQuery<Result<FabricStockLineResponse>>;

public sealed class GetFabricStockLineQueryValidator : AbstractValidator<GetFabricStockLineQuery>
{
    public GetFabricStockLineQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.StockId).GreaterThan(0);
    }
}

public sealed class GetFabricStockLineQueryHandler(IProductionDbContext db, StockReservations reservations,
    StockRows rows) : IQueryHandler<GetFabricStockLineQuery, Result<FabricStockLineResponse>>
{
    public async ValueTask<Result<FabricStockLineResponse>> Handle(GetFabricStockLineQuery query,
        CancellationToken cancellationToken)
    {
        var line = await db.FabricStocks.AsNoTracking().SingleOrDefaultAsync(x => x.Id == query.StockId, cancellationToken);
        if (line is null) return Result<FabricStockLineResponse>.NotFound(StockMessages.LineNotFound);

        return Result.Success(await BuildAsync(db, reservations, rows, query.Actor, line, cancellationToken));
    }

    internal static async Task<FabricStockLineResponse> BuildAsync(IProductionDbContext db,
        StockReservations reservations, StockRows rows, ProductionActor actor, FabricStock line,
        CancellationToken cancellationToken)
    {
        var movements = await db.FabricMovements.AsNoTracking().Where(x => x.StockId == line.Id)
            .OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        var key = new FabricPlanKey(line.FabricCode, line.ColorKey);
        var held = (await reservations.OpenAsync(line.FabricCode, cancellationToken))
            .Where(x => x.Part.Key == key && x.Reserved > 0)
            .OrderBy(x => x.OrderNumber).ThenBy(x => x.ItemId)
            .Select(x => new FabricReservationResponse(x.OrderId, x.OrderNumber, x.ItemId, x.ModelName, x.Part.Meters,
                x.Covered, x.Reserved))
            .ToList();
        var build = await rows.BuilderAsync(actor, cancellationToken);
        return new FabricStockLineResponse(
            build(line, line.FabricCode, line.FabricName, line.Color, held.Sum(x => x.Reserved)),
            movements.Select(x => Map(x, actor)).ToList(), held, StockRows.Permissions(actor));
    }

    internal static FabricMovementResponse Map(FabricMovement x, ProductionActor actor) => new(x.Id, x.Kind.ToString(),
        x.MetersBefore, x.Delta, x.MetersAfter, x.OperationDate, x.Note, x.SupplierId, x.SupplierName, x.Document,
        x.OrderId, x.OrderNumber, x.ModelName, x.ReversesMovementId, x.IsReversed,
        actor.CanMoveStock && !x.IsReversed && x.Kind is FabricMovementKind.Opening or FabricMovementKind.Receipt
            or FabricMovementKind.Adjustment,
        x.CreatedByName, x.CreatedAt);
}
