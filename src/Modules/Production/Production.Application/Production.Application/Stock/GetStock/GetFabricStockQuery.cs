using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Stock;

namespace Production.Application.Stock.GetStock;

/// <summary>
/// «Склад тканин»: every line with бронь and доступно, plus the fabrics and colours open orders plan but the stock
/// never held. <paramref name="Search"/> finds by code, fabric or colour.
/// </summary>
public sealed record GetFabricStockQuery(ProductionActor Actor, string? Search) : IQuery<Result<FabricStockListResponse>>;

public sealed class GetFabricStockQueryValidator : AbstractValidator<GetFabricStockQuery>
{
    public GetFabricStockQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Search).MaximumLength(200);
    }
}

public sealed class GetFabricStockQueryHandler(IProductionDbContext db, StockReservations reservations, StockRows rows)
    : IQueryHandler<GetFabricStockQuery, Result<FabricStockListResponse>>
{
    public async ValueTask<Result<FabricStockListResponse>> Handle(GetFabricStockQuery query,
        CancellationToken cancellationToken)
    {
        var lines = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var reserved = (await reservations.OpenAsync(null, cancellationToken))
            .GroupBy(x => x.Part.Key)
            .ToDictionary(g => g.Key, g => (Reserved: g.Sum(x => x.Reserved), g.First().Part.Color));
        var build = await rows.BuilderAsync(query.Actor, cancellationToken);

        var result = lines.Select(x => build(x, x.FabricCode, x.FabricName, x.Color,
                reserved.GetValueOrDefault(new FabricPlanKey(x.FabricCode, x.ColorKey)).Reserved))
            .ToList();
        var held = lines.Select(x => new FabricPlanKey(x.FabricCode, x.ColorKey)).ToHashSet();
        result.AddRange(reserved.Where(x => !held.Contains(x.Key) && x.Value.Reserved > 0)
            .Select(x => build(null, x.Key.FabricCode, x.Key.FabricCode, x.Value.Color, x.Value.Reserved)));

        var search = query.Search?.Trim();
        var filtered = result
            .Where(x => string.IsNullOrEmpty(search) ||
                        x.FabricCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        x.FabricName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        x.Color.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.FabricCode, StringComparer.Ordinal).ThenBy(x => x.Color, StringComparer.CurrentCulture)
            .ToList();
        var totals = new FabricStockTotals(filtered.Sum(x => x.Meters), filtered.Sum(x => x.Reserved), filtered.Count,
            filtered.Count(x => x.Available < 0),
            query.Actor.SeesStockValue ? filtered.Sum(x => x.Value ?? 0m) : null);
        return Result.Success(new FabricStockListResponse(filtered, totals, StockRows.Permissions(query.Actor)));
    }
}
