using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

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
