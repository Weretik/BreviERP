using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

public sealed class GetFabricValuationHandler(
    IWarehouseDbContext db,
    IWarehouseReferenceReader reference,
    TimeProvider clock) : IQueryHandler<GetFabricValuationQuery, Result<FabricValuationResponse>>
{
    public const string PriceSource = "Ціни довідника «Тканини» на момент розрахунку";

    public async ValueTask<Result<FabricValuationResponse>> Handle(GetFabricValuationQuery query,
        CancellationToken cancellationToken)
    {
        var lines = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var offers = (await reference.FabricsAsync(cancellationToken))
            .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Code)).ToList();

        var fabrics = new List<FabricValuationFabricResponse>();
        var shortages = new List<FabricValuationShortageResponse>();
        foreach (var group in lines.GroupBy(x => FabricAliases.Canonical(x.FabricCode)))
        {
            var price = FabricValuation.Price(group.Key, offers);
            var anchor = group.OrderBy(x => x.FabricCode == group.Key ? 0 : 1).ThenBy(x => x.Id).First();
            var name = FabricAliases.Group(group.Key)?.Name ??
                       offers.FirstOrDefault(x => FabricAliases.Normalize(x.Code!) == group.Key)?.Name ??
                       anchor.FabricName;
            var colors = group.GroupBy(x => StockColor.Key(x.Color))
                .Select(x => (Color: x.First().Color, Meters: x.Sum(l => l.Meters))).ToList();

            foreach (var shortage in colors.Where(x => x.Meters < 0))
                shortages.Add(new FabricValuationShortageResponse(group.Key, name, shortage.Color, -shortage.Meters,
                    price?.Price, price is null ? null : FabricValuation.Value(-shortage.Meters, price.Price)));

            var positive = colors.Where(x => x.Meters > 0)
                .Select(x => new FabricValuationColorResponse(x.Color, x.Meters, price?.Price,
                    price is null ? null : FabricValuation.Value(x.Meters, price.Price)))
                .OrderBy(x => x.Color, StringComparer.CurrentCulture).ToList();
            if (positive.Count == 0) continue;
            fabrics.Add(new FabricValuationFabricResponse(group.Key, name, price?.Price, price?.FabricCode,
                positive.Sum(x => x.Meters), price is null ? positive.Sum(x => x.Meters) : 0m,
                positive.Sum(x => x.Value ?? 0m), positive));
        }

        fabrics = fabrics.OrderBy(x => x.FabricName, StringComparer.CurrentCulture)
            .ThenBy(x => x.FabricCode, StringComparer.Ordinal).ToList();
        var unpriced = fabrics.Where(x => x.Price is null).ToList();
        return Result.Success(new FabricValuationResponse(fabrics.Sum(x => x.Value), fabrics.Sum(x => x.Meters),
            unpriced.Sum(x => x.Colors.Count), unpriced.Sum(x => x.Meters), unpriced.Count > 0,
            shortages.Sum(x => x.Meters), shortages.Sum(x => x.Value ?? 0m),
            fabrics, shortages.OrderBy(x => x.FabricName, StringComparer.CurrentCulture)
                .ThenBy(x => x.Color, StringComparer.CurrentCulture).ToList(),
            clock.GetUtcNow(), PriceSource));
    }
}
