using Production.Contracts.FabricPlans;
using Warehouse.Application.Abstractions;

namespace Warehouse.Infrastructure.Adapters.Production;

/// <summary>«Швейний цех» (Production) for «Склад тканин»: items with their fabric plan, moved orders, item history.</summary>
internal sealed class WarehouseProductionReader(IOrderFabricPlans plans) : IWarehouseProductionReader
{
    public async Task<IReadOnlyList<WarehousePlanItem>> OpenItemsAsync(string? fabricCode,
        CancellationToken cancellationToken) =>
        (await plans.OpenItemsAsync(fabricCode, cancellationToken)).Select(Map).ToArray();

    public async Task<WarehousePlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken) =>
        await plans.ItemAsync(orderId, itemId, cancellationToken) is { } item ? Map(item) : null;

    public async Task<IReadOnlyList<WarehousePlanItem>> ItemsAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken) =>
        (await plans.ItemsAsync(itemIds, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<WarehouseAssemblyItem>> OpenAssembliesAsync(CancellationToken cancellationToken) =>
        (await plans.OpenAssembliesAsync(cancellationToken)).Select(x => new WarehouseAssemblyItem(x.OrderId,
            x.OrderNumber, x.ClientName, x.OrderCreatedAt, x.ItemId, x.ModelName, x.ModelCode, x.Quantity,
            x.Lines?.Select(l => new WarehouseAssemblyLine(l.Kind, l.Code, l.Name, l.Quantity, l.Unit, l.Comment))
                .ToArray(), x.DueDate)).ToArray();

    public async Task<IReadOnlyList<WarehouseLegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken) =>
        (await plans.LegacyOrdersAsync(legacyIds, cancellationToken))
        .Select(x => new WarehouseLegacyOrder(x.LegacyId, x.OrderId, x.Number,
            x.Items.Select(i => new WarehouseLegacyItem(i.Position, i.ItemId, i.ModelName, i.ModelCode, i.Quantity))
                .ToArray()))
        .ToArray();

    public Task RecordFabricAsync(int orderId, int itemId, string text, string actorName,
        CancellationToken cancellationToken) =>
        plans.RecordFabricAsync(orderId, itemId, text, actorName, cancellationToken);

    internal static WarehousePlanItem Map(OrderPlanItem x) => new(x.OrderId, x.OrderNumber, x.OrderVersion,
        x.IsOpen, x.ClientName, x.ItemId, x.ModelName, x.FabricCode, x.FabricName, x.Quantity, x.HasNorm,
        x.Parts.Select(p => new WarehousePlanPart(p.FabricCode, p.Color, p.ColorKey, p.Meters)).ToArray(), x.ModelCode,
        x.OrderCreatedAt, x.DueDate);
}
