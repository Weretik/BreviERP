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

public sealed class GetFabricBookingQueryHandler(FabricBookingCalculator booking)
    : IQueryHandler<GetFabricBookingQuery, Result<FabricBookingResponse>>
{
    public async ValueTask<Result<FabricBookingResponse>> Handle(GetFabricBookingQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await booking.CalculateAsync(cancellationToken));
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
