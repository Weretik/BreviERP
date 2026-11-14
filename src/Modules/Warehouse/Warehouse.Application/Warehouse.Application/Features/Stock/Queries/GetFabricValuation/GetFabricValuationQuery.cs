using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

/// <summary>
/// «Вартість складу» (TZ §13): the positive balance of every fabric and colour at today's price of «Тканини». Each
/// request reads the balances and the prices anew («Оновити ціни і залишки»).
/// </summary>
public sealed record GetFabricValuationQuery(WarehouseActor Actor) : IQuery<Result<FabricValuationResponse>>;

public sealed class GetFabricValuationValidator : AbstractValidator<GetFabricValuationQuery>
{
    public GetFabricValuationValidator() => RuleFor(x => x.Actor).ValidActor();
}

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

/// <summary>The price and value rules of «Вартість складу» (TZ §13).</summary>
public static class FabricValuation
{
    public sealed record GroupPrice(decimal Price, string FabricCode);

    /// <summary>
    /// The lowest positive price among the active offers of the fabric's group (one code outside the reviewed groups).
    /// An offer of a group whose composition or density no longer matches the reviewed one is not the same fabric.
    /// </summary>
    public static GroupPrice? Price(string code, IReadOnlyList<WarehouseFabric> offers)
    {
        var members = FabricAliases.Members(code).ToHashSet(StringComparer.Ordinal);
        return offers
            .Where(x => members.Contains(FabricAliases.Normalize(x.Code!)) && x.Price is > 0 &&
                        FabricAliases.MatchesGroup(x.Code!, x.Composition, x.Density))
            .OrderBy(x => x.Price).ThenBy(x => x.Code, StringComparer.Ordinal)
            .Select(x => new GroupPrice(x.Price!.Value, FabricAliases.Normalize(x.Code!)))
            .FirstOrDefault();
    }

    /// <summary>Kopecks = round(hundredths of a metre × UAH per metre), shown in UAH.</summary>
    public static decimal Value(decimal meters, decimal price)
    {
        var hundredths = decimal.Round(meters * 100m, MidpointRounding.AwayFromZero);
        return decimal.Round(hundredths * price, MidpointRounding.AwayFromZero) / 100m;
    }
}
