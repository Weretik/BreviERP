using Production.Application.Abstractions;
using Production.Application.Orders;
using Production.Domain.Orders;
using Production.Domain.Stock;

namespace Production.Application.Stock;

/// <summary>A planned part of an item and how much of it is still held on the stock (PRD-D29).</summary>
public sealed record ItemReservation(int OrderId, int OrderNumber, int ItemId, string ModelName, FabricPlanPart Part,
    decimal Covered)
{
    public decimal Reserved => Math.Max(0m, Part.Meters - Covered);
}

/// <summary>Plans of items by colour and what issues covered of them.</summary>
public sealed class StockReservations(IProductionDbContext db)
{
    /// <summary>Every planned part of the items of «Нове» orders; <paramref name="fabricCode"/> narrows to one fabric.</summary>
    public async Task<IReadOnlyList<ItemReservation>> OpenAsync(string? fabricCode, CancellationToken cancellationToken)
    {
        var query = db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.Status == ProductionOrderStatus.New && x.DeletedAt == null);
        var orders = await query.ToListAsync(cancellationToken);
        var items = orders.SelectMany(o => o.Items.Where(i => i.FabricCode != null &&
                (fabricCode == null || i.FabricCode == fabricCode)).Select(i => (Order: o, Item: i)))
            .ToList();
        return await ForAsync(items, cancellationToken);
    }

    /// <summary>The planned parts of one item, whatever the order's status.</summary>
    public async Task<IReadOnlyList<ItemReservation>> ItemAsync(ProductionOrder order, ProductionItem item,
        CancellationToken cancellationToken) =>
        await ForAsync([(order, item)], cancellationToken);

    /// <summary>What issues covered of the plan of the given items, by item and planned part.</summary>
    public async Task<Dictionary<(int ItemId, FabricPlanKey Key), decimal>> CoveredAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken)
    {
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines)
            .Where(x => itemIds.Contains(x.ItemId) && x.ReversedAt == null)
            .ToListAsync(cancellationToken);
        return issues.SelectMany(x => x.Lines.Where(l => l.Plan is not null)
                .Select(l => (x.ItemId, Key: l.Plan!, l.CoveredMeters)))
            .GroupBy(x => (x.ItemId, x.Key))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.CoveredMeters));
    }

    private async Task<IReadOnlyList<ItemReservation>> ForAsync(
        IReadOnlyList<(ProductionOrder Order, ProductionItem Item)> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return [];
        var norms = await OrderNorms.ReadAsync(db, items.Select(x => x.Item.ModelCode), cancellationToken);
        var covered = await CoveredAsync(items.Select(x => x.Item.Id).ToArray(), cancellationToken);
        return items.SelectMany(x => FabricPlanParts
                .For(x.Item.ModelCode is null ? null : norms.GetValueOrDefault(x.Item.ModelCode), x.Item.FabricCode,
                    x.Item.MainColor, x.Item.AdditionalColors, x.Item.Quantity)
                .Select(part => new ItemReservation(x.Order.Id, x.Order.Number, x.Item.Id, x.Item.ModelName, part,
                    covered.GetValueOrDefault((x.Item.Id, part.Key)))))
            .ToList();
    }
}
