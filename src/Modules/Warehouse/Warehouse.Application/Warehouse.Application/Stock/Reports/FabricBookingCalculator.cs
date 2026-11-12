using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Application.Stock.Issues;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Reports;

/// <summary>
/// The booking of every fabric and colour (TZ §12): open plans in the order the orders were created take the stock.
/// Shared by «Що замовити» and the «У броні» / «Вільно» columns of «Залишки».
/// </summary>
public sealed class FabricBookingCalculator(
    IWarehouseDbContext db,
    IWarehouseProductionReader production,
    IWarehouseReferenceReader reference,
    StockReservations reservations,
    ItemPlans plans,
    TimeProvider clock)
{
    public async Task<FabricBookingResponse> CalculateAsync(CancellationToken cancellationToken)
    {
        var stock = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var fabrics = (await reference.FabricsAsync(cancellationToken)).Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => FabricAliases.Canonical(x.Code!)).ToDictionary(g => g.Key, g => g.First().Name);
        var items = await production.OpenItemsAsync(null, cancellationToken);
        var ids = items.Select(x => x.ItemId).ToArray();
        var saved = await plans.SavedAsync(ids, cancellationToken);
        var covered = await reservations.CoveredAsync(ids, cancellationToken);
        items = items.Select(x => ItemPlans.Apply(x, saved.GetValueOrDefault(x.ItemId))).ToList();

        var known = fabrics.Keys.Concat(stock.Select(x => FabricAliases.Canonical(x.FabricCode)))
            .ToHashSet(StringComparer.Ordinal);
        var unknown = new List<FabricBookingUnknownResponse>();
        var needs = new List<(WarehousePlanItem Item, WarehousePlanPart Part, decimal Covered)>();
        foreach (var item in items)
        {
            if (ItemPlans.Missing(item) is { } missing)
            {
                unknown.Add(Unknown(item, missing));
                continue;
            }

            foreach (var part in item.Parts)
                if (known.Contains(FabricAliases.Canonical(part.FabricCode)))
                    needs.Add((item, part, covered.GetValueOrDefault((item.ItemId, part.Key))));
                else
                    unknown.Add(Unknown(item, $"Тканини {part.FabricCode} немає в довіднику «Тканини»."));
        }

        var pairs = stock.GroupBy(x => (Code: FabricAliases.Canonical(x.FabricCode), Key: StockColor.Key(x.Color)))
            .ToDictionary(g => g.Key, g => g.ToList());
        var demand = needs.GroupBy(x => (Code: FabricAliases.Canonical(x.Part.FabricCode), x.Part.ColorKey))
            .ToDictionary(g => (g.Key.Code, Key: g.Key.ColorKey), g => g.ToList());

        var rows = pairs.Keys.Union(demand.Keys).Select(key =>
            {
                var lines = pairs.GetValueOrDefault(key) ?? [];
                var queue = (demand.GetValueOrDefault(key) ?? [])
                    .OrderBy(x => x.Item.OrderCreatedAt).ThenBy(x => x.Item.OrderNumber).ThenBy(x => x.Item.ItemId)
                    .ThenBy(x => x.Part.Layer).ToList();
                var allocation = FabricBooking.Allocate(lines.Sum(x => x.Meters),
                    queue.Select(x => Math.Max(0m, x.Part.Meters - x.Covered)).ToList());
                var anchor = lines.OrderBy(x => x.FabricCode == key.Code ? 0 : 1).ThenBy(x => x.Id).FirstOrDefault();
                var name = FabricAliases.Group(key.Code)?.Name ?? fabrics.GetValueOrDefault(key.Code) ??
                    anchor?.FabricName ?? key.Code;
                var color = anchor?.Color ?? queue[0].Part.Color;
                return new FabricBookingPairResponse(key.Code, name, color, allocation.Stock, allocation.Need,
                    allocation.Reserved, allocation.Free, allocation.ToOrder, allocation.Shortage,
                    queue.Select((x, i) => new FabricBookingItemResponse(x.Item.OrderId, x.Item.OrderNumber,
                        x.Item.ClientName, x.Item.ItemId, x.Item.ModelName, x.Item.DueDate, x.Part.Layer.ToString(),
                        x.Part.Meters, x.Covered, allocation.Items[i].Remaining, allocation.Items[i].Reserved,
                        allocation.Items[i].Missing)).ToList());
            })
            .Where(x => x.Stock != 0 || x.Need > 0)
            .OrderByDescending(x => x.ToOrder).ThenBy(x => x.FabricName, StringComparer.CurrentCulture)
            .ThenBy(x => x.Color, StringComparer.CurrentCulture).ToList();

        return new FabricBookingResponse(rows.Sum(x => x.Reserved), rows.Sum(x => x.Free), rows.Sum(x => x.ToOrder),
            rows, unknown.OrderBy(x => x.OrderNumber).ThenBy(x => x.ItemId).ToList(), clock.GetUtcNow());
    }

    private static FabricBookingUnknownResponse Unknown(WarehousePlanItem item, string reason) =>
        new(item.OrderId, item.OrderNumber, item.ClientName, item.ItemId, item.ModelName, reason);
}
