namespace Warehouse.Application.Features.Stock.Shared;

// ── «Бронь і що замовити» (TZ §12) ─────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Бронь і потреба в тканині»: what the plans of open orders still need of each fabric and colour, what the stock holds
/// for them in order of the orders, and what to order. Reads only; nothing is booked.
/// </summary>
/// <param name="CanReceive">The person may record «Прихід» (the stock keepers), offered from a row.</param>
public sealed record FabricBookingResponse(
    decimal Reserved,
    decimal Free,
    decimal ToOrder,
    IReadOnlyList<FabricBookingPairResponse> Pairs,
    IReadOnlyList<FabricBookingUnknownResponse> Unknown,
    DateTimeOffset CalculatedAt,
    bool CanReceive = false);
