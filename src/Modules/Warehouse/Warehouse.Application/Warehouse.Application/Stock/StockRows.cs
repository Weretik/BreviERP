using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Application.Access;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock;

/// <summary>Builds stock rows with reservation, availability and, for those who may see it, the value.</summary>
public sealed class StockRows(IWarehouseReferenceReader reference)
{
    public async Task<Func<FabricStock?, string, string, string, decimal, FabricStockRowResponse>> BuilderAsync(
        WarehouseActor actor, CancellationToken cancellationToken)
    {
        var byCode = (await reference.FabricsAsync(cancellationToken)).Where(x => x.Code != null)
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

    public static FabricStockPermissions Permissions(WarehouseActor actor) =>
        new(actor.CanMoveStock, actor.SeesStockValue);
}
