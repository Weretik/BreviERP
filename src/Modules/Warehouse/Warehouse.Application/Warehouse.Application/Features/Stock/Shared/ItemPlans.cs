using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Plans;

namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>
/// The fabric plan of an item: the saved one once fabric was issued (TZ §5.2), otherwise today's plan from the norms
/// of «Швейний цех».
/// </summary>
public sealed class ItemPlans(IWarehouseDbContext db)
{
    /// <summary>Saved plans of the given items, by item id.</summary>
    public async Task<Dictionary<int, FabricItemPlan>> SavedAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken) =>
        itemIds.Count == 0
            ? []
            : await db.FabricItemPlans.AsNoTracking().Where(x => itemIds.Contains(x.ItemId))
                .ToDictionaryAsync(x => x.ItemId, cancellationToken);

    /// <summary>The saved plan, tracked for a change, or null.</summary>
    public Task<FabricItemPlan?> TrackedAsync(int itemId, CancellationToken cancellationToken) =>
        db.FabricItemPlans.SingleOrDefaultAsync(x => x.ItemId == itemId, cancellationToken);

    /// <summary>The item with its saved plan instead of today's, when there is one.</summary>
    public static WarehousePlanItem Apply(WarehousePlanItem item, FabricItemPlan? saved) =>
        saved is null ? item : item with { Quantity = saved.Quantity, Parts = Parts(saved) };

    public static IReadOnlyList<WarehousePlanPart> Parts(FabricItemPlan plan) =>
        plan.Parts.Select(x => new WarehousePlanPart(x.FabricCode, x.Color, x.ColorKey, x.Meters, x.Layer)).ToList();

    /// <summary>Saves today's plan of the item, for its first issue.</summary>
    public FabricItemPlan Save(WarehousePlanItem item, string actorName, DateTimeOffset now)
    {
        var plan = new FabricItemPlan(item.OrderId, item.ItemId, Math.Max(1, item.Quantity), Parts(item), actorName, now);
        db.FabricItemPlans.Add(plan);
        return plan;
    }

    public static IEnumerable<FabricItemPlanPart> Parts(WarehousePlanItem item) =>
        item.Parts.Select(x => new FabricItemPlanPart(x.FabricCode, x.Color, x.ColorKey, x.Layer, x.Meters));

    /// <summary>Why the item has no plan, or null when it has one.</summary>
    public static string? Missing(WarehousePlanItem item) =>
        item.Parts.Count > 0 ? null
        : string.IsNullOrWhiteSpace(item.FabricCode) ? "У позиції не вказана тканина: план не порахувати."
        : !item.HasNorm ? "Для моделі немає норми витрати: план не порахувати."
        : "Норма моделі не має метрів для кольорів позиції.";
}
