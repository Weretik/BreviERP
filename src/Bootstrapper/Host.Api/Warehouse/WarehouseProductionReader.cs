using Production.Application.FabricPlans;
using Warehouse.Application.Abstractions;

namespace Host.Api.Warehouse;

/// <summary>«Швейний цех» (Production) for «Склад тканин»: items with their fabric plan, moved orders, item history.</summary>
internal sealed class WarehouseProductionReader(ProductionFabricPlans plans) : IWarehouseProductionReader
{
    public async Task<IReadOnlyList<WarehousePlanItem>> OpenItemsAsync(string? fabricCode,
        CancellationToken cancellationToken) =>
        (await plans.OpenAsync(fabricCode, cancellationToken)).Select(Map).ToArray();

    public async Task<WarehousePlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken) =>
        await plans.ItemAsync(orderId, itemId, cancellationToken) is { } item ? Map(item) : null;

    public async Task<IReadOnlyList<WarehouseLegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken) =>
        (await plans.LegacyAsync(legacyIds, cancellationToken))
        .Select(x => new WarehouseLegacyOrder(x.LegacyId, x.OrderId, x.Number,
            x.Items.Select(i => new WarehouseLegacyItem(i.Position, i.ItemId, i.ModelName)).ToArray()))
        .ToArray();

    public Task RecordFabricAsync(int orderId, int itemId, string text, string actorName,
        CancellationToken cancellationToken) =>
        plans.RecordFabricAsync(orderId, itemId, text, actorName, cancellationToken);

    internal static WarehousePlanItem Map(ProductionPlanItem x) => new(x.OrderId, x.OrderNumber, x.OrderVersion,
        x.IsOpen, x.ClientName, x.ItemId, x.ModelName, x.FabricCode, x.FabricName, x.Quantity, x.HasNorm,
        x.Parts.Select(p => new WarehousePlanPart(p.FabricCode, p.Color, p.ColorKey, p.Meters)).ToArray());
}
