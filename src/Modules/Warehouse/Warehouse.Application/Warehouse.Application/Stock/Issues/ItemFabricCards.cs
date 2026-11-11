using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Issues;

/// <summary>
/// Builds the «Тканина» card of an item: the plan by part (saved after the first issue) against issues, the pairs of
/// the stock and what open orders hold of them, and the issues with plan and fact.
/// </summary>
public sealed class ItemFabricCards(IWarehouseDbContext db, StockReservations reservations, ItemPlans plans)
{
    public async Task<ProductionItemFabricResponse> BuildAsync(WarehousePlanItem item, WarehouseActor actor,
        CancellationToken cancellationToken)
    {
        var saved = await plans.SavedAsync([item.ItemId], cancellationToken);
        var plan = saved.GetValueOrDefault(item.ItemId);
        var planned = ItemPlans.Apply(item, plan);
        var parts = await reservations.ItemAsync(item, cancellationToken);
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines).ThenInclude(x => x.Movements)
            .Where(x => x.ItemId == item.ItemId).OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        var codes = parts.SelectMany(x => FabricAliases.Members(x.Part.FabricCode)).Distinct().ToArray();
        var lines = codes.Length == 0
            ? []
            : await db.FabricStocks.AsNoTracking().Where(x => codes.Contains(x.FabricCode)).ToListAsync(cancellationToken);
        var held = (await reservations.OpenAsync(null, cancellationToken))
            .GroupBy(x => (Code: FabricAliases.Canonical(x.Part.FabricCode), x.Part.ColorKey))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Reserved));
        var active = issues.Where(x => !x.IsReversed).SelectMany(x => x.Lines).ToList();

        var rows = parts.Select(x =>
        {
            var pair = new StockPair(x.Part.FabricCode, x.Part.ColorKey, lines);
            var mine = item.IsOpen ? x.Reserved : 0m;
            var issued = active.Where(l => l.Plan == x.Part.Key).Sum(l => l.Meters);
            var stock = pair.Meters;
            return new FabricPlanPartResponse(x.Part.FabricCode, x.Part.Color, x.Part.ColorKey, x.Part.Layer.ToString(),
                x.Part.Meters, x.Covered, issued, mine, pair.Lines.FirstOrDefault()?.Id, stock,
                stock - held.GetValueOrDefault((pair.Code, pair.ColorKey)), Math.Max(0m, x.Part.Meters - x.Covered));
        }).ToList();
        return new ProductionItemFabricResponse(item.OrderId, item.ItemId, item.OrderNumber, item.ClientName,
            item.ModelName, item.OrderVersion, item.IsOpen, item.FabricCode, item.FabricName, planned.Quantity,
            item.HasNorm, plan?.Version ?? 0, ItemPlans.Missing(planned), rows, active.Sum(x => x.Meters),
            issues.Select(Map).ToList(), actor.CanIssueFabric(item));
    }

    private static FabricIssueResponse Map(FabricIssue x) => new(x.Id, x.IssuedOn, x.Note, x.CreatedByName,
        x.CreatedAt, x.IsReversed, x.ReversedAt, x.ReversedByName, x.ReverseReason, x.IssuedQuantity, Comparison(x),
        x.Lines.OrderBy(l => l.Id).Select(l => new FabricIssueLineResponse(l.Id, l.Purpose.ToString(), l.PlanFabricCode,
            l.PlanColorKey, l.PlannedMeters, l.StockId, l.FabricCode, l.FabricName, l.Color, l.Meters, l.CoveredMeters,
            l.Layer?.ToString(), l.Note, l.IsVoided)).ToList());

    /// <summary>Plan and fact by planned part: the plan once, the metres of every line of the part summed.</summary>
    public static IReadOnlyList<FabricIssueComparisonResponse> Comparison(FabricIssue issue)
    {
        if (issue.IssuedQuantity is not { } cut) return [];
        return issue.Lines.Where(x => x.Plan is not null).GroupBy(x => x.Plan!).Select(g =>
        {
            var plan = g.First().PlannedMeters ?? 0m;
            var actual = g.Sum(x => x.Meters);
            var color = g.FirstOrDefault(x => x.Purpose == FabricIssuePurpose.Planned)?.Color ?? g.Key.ColorKey;
            return new FabricIssueComparisonResponse(g.Key.FabricCode, g.Key.ColorKey, color, plan, actual,
                actual - plan, decimal.Round(actual / cut, 3));
        }).ToList();
    }
}
