using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Domain.Stock;

namespace Production.Application.Stock;

/// <summary>Builds stock rows with reservation, availability and, for those who may see it, the value.</summary>
public sealed class StockRows(IProductionFabricReader fabrics)
{
    public async Task<Func<FabricStock?, string, string, string, decimal, FabricStockRowResponse>> BuilderAsync(
        ProductionActor actor, CancellationToken cancellationToken)
    {
        var byCode = (await fabrics.ListAsync(cancellationToken)).Where(x => x.Code != null)
            .GroupBy(x => x.Code!.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First());
        return (line, code, name, color, reserved) =>
        {
            var meters = line?.Meters ?? 0m;
            var fabric = byCode.GetValueOrDefault(code);
            decimal? price = actor.SeesStockValue ? fabric?.Price : null;
            decimal? value = price is { } p ? decimal.Round(Math.Max(0m, meters) * p, 2) : null;
            return new FabricStockRowResponse(line?.Id, line?.FabricId ?? fabric?.Id, code, line?.FabricName ?? fabric?.Name ?? name,
                line?.Color ?? color, meters, reserved, meters - reserved, price, value, line?.Version ?? 0,
                line?.UpdatedAt);
        };
    }

    public static FabricStockPermissions Permissions(ProductionActor actor) =>
        new(actor.CanMoveStock, actor.SeesStockValue);
}
