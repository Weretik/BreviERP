using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Issues;

/// <summary>Builds the «Тканина» tab of an item: plan by colour against issues and the stock.</summary>
public sealed class ItemFabricCards(IWarehouseDbContext db, StockReservations reservations)
{
    public async Task<ProductionItemFabricResponse> BuildAsync(WarehousePlanItem item, WarehouseActor actor,
        CancellationToken cancellationToken)
    {
        var plan = await reservations.ItemAsync(item, cancellationToken);
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines).Where(x => x.ItemId == item.ItemId)
            .OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        var codes = plan.Select(x => x.Part.FabricCode).Distinct().ToArray();
        var lines = await db.FabricStocks.AsNoTracking().Where(x => codes.Contains(x.FabricCode))
            .ToListAsync(cancellationToken);
        var openReserved = item.IsOpen
            ? (await reservations.OpenAsync(item.FabricCode, cancellationToken))
                .GroupBy(x => x.Part.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Reserved))
            : await AllReservedAsync(codes, cancellationToken);
        var active = issues.Where(x => !x.IsReversed).SelectMany(x => x.Lines).ToList();

        var parts = plan.Select(x =>
        {
            var line = lines.FirstOrDefault(l => l.FabricCode == x.Part.FabricCode && l.ColorKey == x.Part.ColorKey);
            var mine = item.IsOpen ? x.Reserved : 0m;
            var issued = active.Where(l => l.Plan == x.Part.Key).Sum(l => l.Meters);
            var stock = line?.Meters ?? 0m;
            return new FabricPlanPartResponse(x.Part.FabricCode, x.Part.Color, x.Part.ColorKey, x.Part.Meters,
                x.Covered, issued, mine, line?.Id, stock, stock - openReserved.GetValueOrDefault(x.Part.Key));
        }).ToList();
        return new ProductionItemFabricResponse(item.OrderId, item.ItemId, item.OrderVersion, item.FabricCode,
            item.FabricName, item.Quantity, item.HasNorm, parts, active.Sum(x => x.Meters), issues.Select(Map).ToList(),
            actor.CanIssueFabric(item));
    }

    private async Task<Dictionary<FabricPlanKey, decimal>> AllReservedAsync(IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        if (codes.Count == 0) return [];
        return (await reservations.OpenAsync(null, cancellationToken)).Where(x => codes.Contains(x.Part.FabricCode))
            .GroupBy(x => x.Part.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Reserved));
    }

    private static FabricIssueResponse Map(FabricIssue x) => new(x.Id, x.IssuedOn, x.Note, x.CreatedByName,
        x.CreatedAt, x.IsReversed, x.ReversedAt, x.ReversedByName, x.ReverseReason,
        x.Lines.OrderBy(l => l.Id).Select(l => new FabricIssueLineResponse(l.Id, l.Purpose.ToString(), l.PlanColorKey,
            l.PlannedMeters, l.StockId, l.FabricCode, l.FabricName, l.Color, l.Meters, l.CoveredMeters)).ToList());
}
