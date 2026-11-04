using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Domain.Orders;
using Production.Domain.Stock;

namespace Production.Application.Stock.Issues;

/// <summary>Builds the «Тканина» tab of an item: plan by colour against issues and the stock.</summary>
public sealed class ItemFabricCards(IProductionDbContext db, StockReservations reservations)
{
    public async Task<ProductionItemFabricResponse> BuildAsync(ProductionOrder order, ProductionItem item,
        ProductionActor actor, CancellationToken cancellationToken)
    {
        var plan = await reservations.ItemAsync(order, item, cancellationToken);
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines).Where(x => x.ItemId == item.Id)
            .OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        var codes = plan.Select(x => x.Part.FabricCode).Distinct().ToArray();
        var lines = await db.FabricStocks.AsNoTracking().Where(x => codes.Contains(x.FabricCode))
            .ToListAsync(cancellationToken);
        var openReserved = order.Status == ProductionOrderStatus.New && !order.IsDeleted
            ? (await reservations.OpenAsync(item.FabricCode, cancellationToken))
                .GroupBy(x => x.Part.Key).ToDictionary(g => g.Key, g => g.Sum(x => x.Reserved))
            : await AllReservedAsync(codes, cancellationToken);
        var active = issues.Where(x => !x.IsReversed).SelectMany(x => x.Lines).ToList();

        var parts = plan.Select(x =>
        {
            var line = lines.FirstOrDefault(l => l.FabricCode == x.Part.FabricCode && l.ColorKey == x.Part.ColorKey);
            var mine = order.Status == ProductionOrderStatus.New && !order.IsDeleted ? x.Reserved : 0m;
            var issued = active.Where(l => l.Plan == x.Part.Key).Sum(l => l.Meters);
            var stock = line?.Meters ?? 0m;
            return new FabricPlanPartResponse(x.Part.FabricCode, x.Part.Color, x.Part.ColorKey, x.Part.Meters,
                x.Covered, issued, mine, line?.Id, stock, stock - openReserved.GetValueOrDefault(x.Part.Key));
        }).ToList();
        var norms = await OrderNorms.ReadAsync(db, [item.ModelCode], cancellationToken);
        return new ProductionItemFabricResponse(order.Id, item.Id, order.Version, item.FabricCode, item.FabricName,
            item.Quantity, item.ModelCode is not null && norms.ContainsKey(item.ModelCode), parts,
            active.Sum(x => x.Meters), issues.Select(Map).ToList(), actor.CanIssueFabric(order));
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
