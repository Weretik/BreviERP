using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Plans;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;
using Warehouse.Application.Features.Stock.Shared.Plans;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

/// <summary>The rules of «План і факт» (TZ §11.2–11.3).</summary>
public static class FabricPlanFact
{
    public const string Economy = "Economy";
    public const string Overrun = "Overrun";
    public const string OnPlan = "OnPlan";
    public const string NoPlan = "NoPlan";
    public const string NeedsCheck = "NeedsCheck";

    public const string MissingProducts = "Є видача без кількості виробів.";
    public const string TooManyProducts = "Видано на більше виробів, ніж у позиції.";
    public const string NotIssued = "Основну тканину плану не зафіксовано у видачах.";
    public const string UnknownRole = "Є додаткова тканина без ролі.";
    public const string OldPlan = "Є видача за частиною плану, якої вже немає.";

    /// <summary>
    /// One item: its saved plan by layer, the active (not voided) lines of its issues, and whether its main fabric can be
    /// compared. Fact is the issued main fabric: lines of the planned main parts (planned or replacements) and
    /// additional lines given the main role.
    /// </summary>
    public static FabricPlanFactEntry Evaluate(WarehousePlanItem item, FabricItemPlan? saved,
        IReadOnlyList<FabricIssue> issues, Func<string, string?> fabricName)
    {
        var products = saved?.Quantity ?? item.Quantity;
        var parts = saved is null ? item.Parts : ItemPlans.Parts(saved);
        var lines = issues.SelectMany(x => x.Lines).Where(x => !x.IsVoided).ToList();

        var counts = issues.Select(x => x.IssuedQuantity).ToList();
        var cut = counts.Sum(x => x ?? 0);
        string? reason = counts.Any(x => x is null) ? MissingProducts : cut > products ? TooManyProducts : null;
        var inProgress = reason is null && cut < products;

        var upper = parts.Where(x => x.Layer == FabricLayer.Upper).ToList();
        var materials = new List<FabricPlanFactMaterialResponse>();
        foreach (var part in parts)
        {
            var linked = lines.Where(l => l.Plan == part.Key).ToList();
            decimal? issued = linked.Count == 0 ? null : linked.Sum(Active);
            materials.Add(new FabricPlanFactMaterialResponse(part.Layer.ToString(), part.FabricCode,
                fabricName(part.FabricCode), part.Color, part.Meters, issued, issued - part.Meters, Used(linked)));
        }

        var keys = parts.Select(x => x.Key).ToHashSet();
        var orphans = lines.Where(l => l.Plan is { } key && !keys.Contains(key)).ToList();
        var additional = lines.Where(l => l.Plan is null).ToList();
        foreach (var group in orphans.Concat(additional).GroupBy(l => l.Layer))
            materials.Add(new FabricPlanFactMaterialResponse(group.Key?.ToString(), null, null, null, null,
                group.Sum(Active), null, Used(group)));

        var upperKeys = upper.Select(x => x.Key).ToHashSet();
        var fact = lines.Where(l => l.Plan is { } key ? upperKeys.Contains(key) : l.Layer == FabricLayer.Upper)
            .Sum(Active);
        reason ??= upper.Any(p => lines.All(l => l.Plan != p.Key)) ? NotIssued
            : additional.Any(l => l.Layer is null) ? UnknownRole
            : orphans.Count > 0 ? OldPlan
            : null;
        decimal? plan = upper.Count == 0 ? null : upper.Sum(x => x.Meters);
        var result = upper.Count == 0 ? NoPlan : reason is not null ? NeedsCheck : Outcome(fact - plan!.Value);
        decimal? difference = result is NoPlan or NeedsCheck ? null : fact - plan;

        return new FabricPlanFactEntry(item.ModelName, item.ModelCode, inProgress,
            new FabricPlanFactItemResponse(item.OrderId, item.OrderNumber, item.ClientName, item.ItemId, products, plan,
                fact, difference, result, result == NoPlan ? null : reason, materials));
    }

    /// <summary>
    /// Totals of evaluated items: per product weighted by products (110 m for 30 products is 3.667, not the mean of the
    /// norms); one item that cannot be compared makes the difference unknown.
    /// </summary>
    public static FabricPlanFactRowResponse Summary(IReadOnlyList<FabricPlanFactEntry> entries)
    {
        var items = entries.Select(x => x.Item).ToList();
        var products = items.Sum(x => x.Products);
        var planned = items.Where(x => x.Plan is not null).ToList();
        var plannedProducts = planned.Sum(x => x.Products);
        var fact = items.Sum(x => x.Fact);
        var uncomparable = items.Count(x => x.Result is NoPlan or NeedsCheck);
        decimal? plan = planned.Count == 0 ? null : planned.Sum(x => x.Plan!.Value);
        decimal? difference = uncomparable > 0 || plan is null ? null : fact - plan.Value;
        var result = difference is { } d ? Outcome(d)
            : items.Count > 0 && items.All(x => x.Result == NoPlan) ? NoPlan
            : NeedsCheck;
        return new FabricPlanFactRowResponse(items.Select(x => x.OrderId).Distinct().Count(), products,
            plannedProducts == 0 ? null : PerProduct(plan!.Value, plannedProducts), PerProduct(fact, products),
            difference is { } diff ? PerProduct(diff, products) : null, difference, result, uncomparable);
    }

    public static string Outcome(decimal difference) => difference < 0 ? Economy : difference > 0 ? Overrun : OnPlan;

    private static decimal PerProduct(decimal meters, int products) =>
        products <= 0 ? 0m : decimal.Round(meters / products, 3, MidpointRounding.AwayFromZero);

    private static decimal Active(FabricIssueLine line) => -line.Movements.Where(x => !x.IsVoided).Sum(x => x.Delta);

    private static IReadOnlyList<string> Used(IEnumerable<FabricIssueLine> lines) =>
        lines.Select(x => $"{x.FabricCode} {x.Color}").Distinct(StringComparer.Ordinal).ToList();
}
