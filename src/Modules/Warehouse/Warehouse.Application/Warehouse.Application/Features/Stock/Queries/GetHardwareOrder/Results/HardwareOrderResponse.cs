namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder.Results;

// ── «Фурнітура до замовлення» (TZ §14) ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Фурнітура до замовлення»: the full need of physical accessories of every open order, by the composition of the
/// models. Stock and issues are not taken off.
/// </summary>
/// <param name="Orders">Open orders.</param>
/// <param name="Items">Their items (models).</param>
/// <param name="Incomplete">Lines whose quantity is unknown: «Потреба неповна».</param>
public sealed record HardwareOrderResponse(
    int Orders,
    int Items,
    IReadOnlyList<HardwareNeedResponse> Needs,
    IReadOnlyList<HardwareIncompleteResponse> Incomplete,
    DateTimeOffset CalculatedAt);
