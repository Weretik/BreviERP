using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Application.Access;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Consumption;

/// <summary>
/// «Зведення витрати»: received, issued and corrected metres of each line over a period, by operation date. A reversal
/// counts against the kind it takes back; taking back an opening counts as a correction.
/// </summary>
public sealed record GetFabricConsumptionQuery(WarehouseActor Actor, DateOnly From, DateOnly To)
    : IQuery<Result<FabricConsumptionResponse>>;

public sealed class GetFabricConsumptionQueryValidator : AbstractValidator<GetFabricConsumptionQuery>
{
    public GetFabricConsumptionQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("Кінець періоду раніше за початок.");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber <= 3660).WithMessage("Період до 10 років.");
    }
}

public sealed class GetFabricConsumptionQueryHandler(IWarehouseDbContext db, IWarehouseReferenceReader reference)
    : IQueryHandler<GetFabricConsumptionQuery, Result<FabricConsumptionResponse>>
{
    public async ValueTask<Result<FabricConsumptionResponse>> Handle(GetFabricConsumptionQuery query,
        CancellationToken cancellationToken)
    {
        var movements = await db.FabricMovements.AsNoTracking()
            .Where(x => x.OperationDate >= query.From && x.OperationDate <= query.To && x.Kind != FabricMovementKind.Opening)
            .ToListAsync(cancellationToken);
        var reversedIds = movements.Where(x => x.ReversesMovementId != null).Select(x => x.ReversesMovementId!.Value)
            .ToArray();
        var reversedKinds = await db.FabricMovements.AsNoTracking().Where(x => reversedIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Kind, cancellationToken);
        var stockIds = movements.Select(x => x.StockId).Distinct().ToArray();
        var lines = await db.FabricStocks.AsNoTracking().Where(x => stockIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var prices = query.Actor.SeesStockValue
            ? (await reference.FabricsAsync(cancellationToken)).Where(x => x.Code != null && x.Price != null)
                .GroupBy(x => x.Code!.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Price!.Value)
            : [];

        FabricMovementKind Effective(FabricMovement x) => x.Kind == FabricMovementKind.Reversal &&
                                                          x.ReversesMovementId is { } id &&
                                                          reversedKinds.TryGetValue(id, out var kind)
            ? kind
            : x.Kind;

        var rows = movements.GroupBy(x => x.StockId).Select(g =>
            {
                var line = lines[g.Key];
                var issued = -g.Where(x => Effective(x) == FabricMovementKind.Issue).Sum(x => x.Delta);
                decimal? value = query.Actor.SeesStockValue && prices.TryGetValue(line.FabricCode, out var price)
                    ? decimal.Round(issued * price, 2)
                    : null;
                return new FabricConsumptionRowResponse(line.Id, line.FabricCode, line.FabricName, line.Color,
                    g.Where(x => Effective(x) == FabricMovementKind.Receipt).Sum(x => x.Delta), issued,
                    g.Where(x => Effective(x) is FabricMovementKind.Adjustment or FabricMovementKind.Opening
                        or FabricMovementKind.Reversal).Sum(x => x.Delta),
                    value);
            })
            .OrderByDescending(x => x.Issued).ThenBy(x => x.FabricCode, StringComparer.Ordinal).ToList();
        return Result.Success(new FabricConsumptionResponse(query.From, query.To, rows, rows.Sum(x => x.Received),
            rows.Sum(x => x.Issued), query.Actor.SeesStockValue ? rows.Sum(x => x.IssuedValue ?? 0m) : null));
    }
}
