using Production.Application.Abstractions;
using Production.Domain.FabricPlans;
using Production.Domain.Orders;
using Production.Contracts.FabricPlans;
using Production.Application.Features.Orders.Shared;
using Production.Domain.Orders.Enums;
using Production.Application.Features.Orders.Shared.Rules;

namespace Production.Application.Features.FabricPlans.Shared;

/// <summary>
/// What «Склад тканин» (Warehouse) needs of the workshop: the plan of items by colour from the norms (PRD-D28), the
/// orders moved from the old «Цех», and the line «Тканина» in an item's history. Other modules read it through `Production.Contracts`.
/// </summary>
public sealed class ProductionFabricPlans(IProductionDbContext db, TimeProvider clock, ProductionAssemblies assemblies)
    : IOrderFabricPlans
{
    /// <summary>Items of «Нове» orders; <paramref name="fabricCode"/> narrows to the items that plan that fabric.</summary>
    public async Task<IReadOnlyList<OrderPlanItem>> OpenItemsAsync(string? fabricCode,
        CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.Status == ProductionOrderStatus.New && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
        var items = orders.SelectMany(o => o.Items.Where(i => fabricCode == null || i.FabricCode == fabricCode)
                .Select(i => (Order: o, Item: i)))
            .ToList();
        return await PlanAsync(items, cancellationToken);
    }

    /// <summary>One item of an order that is not deleted, whatever its status.</summary>
    public async Task<OrderPlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == orderId && x.DeletedAt == null, cancellationToken);
        var item = order?.Items.FirstOrDefault(x => x.Id == itemId);
        if (order is null || item is null) return null;
        return (await PlanAsync([(order, item)], cancellationToken))[0];
    }

    /// <summary>The given items of orders that are not deleted, whatever their status; unknown ids are left out.</summary>
    public async Task<IReadOnlyList<OrderPlanItem>> ItemsAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0) return [];
        var ids = itemIds.Distinct().ToArray();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.DeletedAt == null && x.Items.Any(i => ids.Contains(i.Id)))
            .ToListAsync(cancellationToken);
        var items = orders.SelectMany(o => o.Items.Where(i => ids.Contains(i.Id)).Select(i => (Order: o, Item: i)))
            .ToList();
        return await PlanAsync(items, cancellationToken);
    }

    /// <summary>Orders moved from the old «Цех» that are not deleted, by their old id.</summary>
    public async Task<IReadOnlyList<LegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken)
    {
        var ids = legacyIds.ToArray();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.LegacyId != null && ids.Contains(x.LegacyId) && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
        return orders.Select(o => new LegacyOrder(o.LegacyId!, o.Id, o.Number,
            o.Items.Select(i => new LegacyOrderItem(i.Position, i.Id, i.ModelName, i.ModelCode, i.Quantity)).ToList())).ToList();
    }

    public Task<IReadOnlyList<AssemblyItem>> OpenAssembliesAsync(CancellationToken cancellationToken) =>
        assemblies.OpenAsync(cancellationToken);

    /// <summary>Adds «Тканина» to the history of the item; nothing when the order is gone.</summary>
    public async Task RecordFabricAsync(int orderId, int itemId, string text, string actorName,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return;
        order.RecordAbout(itemId, "Тканина", null, text, actorName, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<OrderPlanItem>> PlanAsync(
        IReadOnlyList<(ProductionOrder Order, ProductionItem Item)> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return [];
        var norms = await OrderNorms.ReadAsync(db, items.Select(x => x.Item.ModelCode), cancellationToken);
        return items.Select(x =>
        {
            var norm = x.Item.ModelCode is null ? null : norms.GetValueOrDefault(x.Item.ModelCode);
            return new OrderPlanItem(x.Order.Id, x.Order.Number, x.Order.Version,
                x.Order is { Status: ProductionOrderStatus.New, IsDeleted: false }, x.Order.ClientName, x.Item.Id,
                x.Item.ModelName,
                x.Item.FabricCode, x.Item.FabricName, x.Item.Quantity, norm is not null,
                FabricPlanParts.For(norm, x.Item.FabricCode, x.Item.MainColor, x.Item.AdditionalColors,
                        x.Item.Quantity)
                    .Select(p => new OrderPlanPart(p.FabricCode, p.Color, p.ColorKey, p.Meters)).ToList(),
                x.Item.ModelCode, x.Order.CreatedAt, x.Item.DueDate);
        }).ToList();
    }
}
