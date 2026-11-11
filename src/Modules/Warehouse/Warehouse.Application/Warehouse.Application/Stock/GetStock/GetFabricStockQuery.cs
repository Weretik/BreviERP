using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.GetStock;

/// <summary>
/// «Залишки»: fabrics by canonical code with every colour the stock holds, lines of an alias group and colour summed.
/// Only fabrics and colours with stock lines appear; zero and negative balances stay. Search is on the page.
/// </summary>
public sealed record GetFabricStockQuery(WarehouseActor Actor) : IQuery<Result<FabricStockListResponse>>;

public sealed class GetFabricStockQueryValidator : AbstractValidator<GetFabricStockQuery>
{
    public GetFabricStockQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetFabricStockQueryHandler(IWarehouseDbContext db, IWarehouseReferenceReader reference)
    : IQueryHandler<GetFabricStockQuery, Result<FabricStockListResponse>>
{
    public async ValueTask<Result<FabricStockListResponse>> Handle(GetFabricStockQuery query,
        CancellationToken cancellationToken)
    {
        var lines = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var fabrics = (await reference.FabricsAsync(cancellationToken)).Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => FabricAliases.Normalize(x.Code!)).ToDictionary(g => g.Key, g => g.First());

        var result = lines.GroupBy(x => FabricAliases.Canonical(x.FabricCode)).Select(group =>
            {
                var colors = group.GroupBy(x => StockColor.Key(x.Color))
                    .Select(x => new StockPair(group.Key, x.Key, x))
                    .Select(pair => new FabricStockColorResponse(pair.Lines[0].Color, pair.ColorKey, pair.Meters,
                        pair.Version, pair.Lines.Select(l => new FabricStockSourceResponse(l.Id, l.FabricCode, l.Meters))
                            .ToList()))
                    .OrderBy(x => x.Color, StringComparer.CurrentCulture).ToList();
                var alias = FabricAliases.Group(group.Key);
                var fabric = fabrics.GetValueOrDefault(group.Key);
                var anchor = group.OrderBy(x => x.FabricCode == group.Key ? 0 : 1).ThenBy(x => x.Id).First();
                return new FabricStockFabricResponse(group.Key, FabricAliases.Members(group.Key),
                    alias?.Name ?? fabric?.Name ?? anchor.FabricName, alias?.Composition ?? fabric?.Composition,
                    alias?.Density ?? fabric?.Density, colors.Sum(x => x.Meters), colors);
            })
            .OrderBy(x => x.FabricName, StringComparer.CurrentCulture).ThenBy(x => x.FabricCode, StringComparer.Ordinal)
            .ToList();
        var pairs = result.SelectMany(x => x.Colors).ToList();
        return Result.Success(new FabricStockListResponse(result, pairs.Sum(x => x.Meters), pairs.Count,
            pairs.Count(x => x.Meters < 0), StockList.Permissions(query.Actor)));
    }
}
