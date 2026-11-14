using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Plans;
using Warehouse.Application.Features.Stock.Shared.Plans;

namespace Warehouse.Application.Features.Stock.Shared.Operations;

/// <summary>A planned part of an item and how much of it is still held on the stock (PRD-D29).</summary>
public sealed record ItemReservation(int OrderId, int OrderNumber, int ItemId, string ModelName, WarehousePlanPart Part,
    decimal Covered)
{
    public decimal Reserved => Math.Max(0m, Part.Meters - Covered);
}

/// <summary>
/// Plans of items by colour (the saved plan after the first issue, otherwise from «Швейний цех») and what issues of
/// this stock covered of them.
/// </summary>
public sealed class StockReservations(IWarehouseDbContext db, IWarehouseProductionReader production, ItemPlans plans)
{
    /// <summary>Every planned part of the items of «Нове» orders; <paramref name="fabricCode"/> narrows to one fabric.</summary>
    public async Task<IReadOnlyList<ItemReservation>> OpenAsync(string? fabricCode, CancellationToken cancellationToken) =>
        await ForAsync(await production.OpenItemsAsync(fabricCode, cancellationToken), cancellationToken);

    /// <summary>The planned parts of one item, whatever the order's status.</summary>
    public async Task<IReadOnlyList<ItemReservation>> ItemAsync(WarehousePlanItem item,
        CancellationToken cancellationToken) =>
        await ForAsync([item], cancellationToken);

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

    private async Task<IReadOnlyList<ItemReservation>> ForAsync(IReadOnlyList<WarehousePlanItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return [];
        var ids = items.Select(x => x.ItemId).ToArray();
        var covered = await CoveredAsync(ids, cancellationToken);
        var saved = await plans.SavedAsync(ids, cancellationToken);
        items = items.Select(x => ItemPlans.Apply(x, saved.GetValueOrDefault(x.ItemId))).ToList();
        return items.SelectMany(x => x.Parts.Select(part => new ItemReservation(x.OrderId, x.OrderNumber, x.ItemId,
                x.ModelName, part, covered.GetValueOrDefault((x.ItemId, part.Key)))))
            .ToList();
    }
}
