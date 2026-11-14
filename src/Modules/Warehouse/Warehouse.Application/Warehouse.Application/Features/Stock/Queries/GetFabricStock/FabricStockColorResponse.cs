namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

/// <summary>
/// A fabric and colour on the shelf: every line of the alias group with that colour summed. <c>Version</c> goes back
/// with «Уточнити» and a «Разова витрата».
/// </summary>
/// <param name="Booked">«У броні»: what open production plans still need of this pair (plan − covered by issues).</param>
/// <param name="Free">«Вільно»: metres − booked; below zero when the stock does not cover the plans.</param>
/// <param name="ToOrder">What to buy for the plans, the shortage already there included (as «Що замовити»).</param>
/// <param name="LastMovement">The latest movement that counts (not voided) by operation date; null without one.</param>
/// <param name="Bookings">The tasks that need this pair, oldest order first.</param>
public sealed record FabricStockColorResponse(
    string Color,
    string ColorKey,
    decimal Meters,
    string Version,
    IReadOnlyList<FabricStockSourceResponse> Sources,
    decimal Booked = 0,
    decimal Free = 0,
    decimal ToOrder = 0,
    FabricStockLastMovementResponse? LastMovement = null,
    IReadOnlyList<FabricStockBookingResponse>? Bookings = null);
