using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Application.Stock.Issues;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Reports;

/// <summary>
/// «Бронь і що замовити» (TZ §12): for every fabric and colour, the stock is handed to the plans of open orders in the
/// order the orders were created; what is left over is free, what is missing is to order. Reads only.
/// </summary>
public sealed record GetFabricBookingQuery(WarehouseActor Actor) : IQuery<Result<FabricBookingResponse>>;

public sealed class GetFabricBookingQueryValidator : AbstractValidator<GetFabricBookingQuery>
{
    public GetFabricBookingQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetFabricBookingQueryHandler(
    IWarehouseDbContext db,
    IWarehouseProductionReader production,
    IWarehouseReferenceReader reference,
    StockReservations reservations,
    ItemPlans plans,
    TimeProvider clock) : IQueryHandler<GetFabricBookingQuery, Result<FabricBookingResponse>>
{
    public async ValueTask<Result<FabricBookingResponse>> Handle(GetFabricBookingQuery query,
        CancellationToken cancellationToken)
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

        return Result.Success(new FabricBookingResponse(rows.Sum(x => x.Reserved), rows.Sum(x => x.Free),
            rows.Sum(x => x.ToOrder), rows,
            unknown.OrderBy(x => x.OrderNumber).ThenBy(x => x.ItemId).ToList(), clock.GetUtcNow()));
    }

    private static FabricBookingUnknownResponse Unknown(WarehousePlanItem item, string reason) =>
        new(item.OrderId, item.OrderNumber, item.ClientName, item.ItemId, item.ModelName, reason);
}

/// <summary>
/// The allocation of one fabric and colour (TZ §12.1), in whole hundredths of a metre: A starts at max(0, S); each
/// item in turn books B = min(A, R) and misses M = R − B. Free = max(0, S − ΣB); to order = max(0, −S) + ΣM.
/// 100 m for 70 m and 50 m books 70 and 30, frees 0 and orders 20; −10 m for 20 m orders 30.
/// </summary>
public static class FabricBooking
{
    public sealed record Share(decimal Remaining, decimal Reserved, decimal Missing);

    public sealed record Allocation(decimal Stock, decimal Need, decimal Reserved, decimal Free, decimal ToOrder,
        decimal Shortage, IReadOnlyList<Share> Items);

    public static Allocation Allocate(decimal stock, IReadOnlyList<decimal> remaining)
    {
        var s = Hundredths(stock);
        var available = Math.Max(0L, s);
        var shares = new List<Share>(remaining.Count);
        long booked = 0, missing = 0, need = 0;
        foreach (var r in remaining.Select(x => Math.Max(0L, Hundredths(x))))
        {
            var b = Math.Min(available, r);
            available -= b;
            booked += b;
            missing += r - b;
            need += r;
            shares.Add(new Share(Meters(r), Meters(b), Meters(r - b)));
        }

        return new Allocation(Meters(s), Meters(need), Meters(booked), Meters(Math.Max(0L, s - booked)),
            Meters(Math.Max(0L, -s) + missing), Meters(Math.Max(0L, -s)), shares);
    }

    private static long Hundredths(decimal meters) => (long)decimal.Round(meters * 100m, MidpointRounding.AwayFromZero);

    private static decimal Meters(long hundredths) => hundredths / 100m;
}
