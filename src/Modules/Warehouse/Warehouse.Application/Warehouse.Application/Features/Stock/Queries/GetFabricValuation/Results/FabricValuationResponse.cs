namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation.Results;

// ── «Вартість складу» (TZ §13) ─────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// «Вартість тканин на складі»: the positive balance of every pair at today's price of «Тканини» (the lowest positive
/// price of the fabric group). Not FIFO and not the purchase cost. Shortages are listed apart and do not lower it.
/// </summary>
/// <param name="Value">Sum of the priced pairs, UAH.</param>
/// <param name="IsPartial">Some positive pairs have no price: <see cref="Value"/> is of the priced part only.</param>
public sealed record FabricValuationResponse(
    decimal Value,
    decimal PositiveMeters,
    int UnpricedPairs,
    decimal UnpricedMeters,
    bool IsPartial,
    decimal ShortageMeters,
    decimal ShortageValue,
    IReadOnlyList<FabricValuationFabricResponse> Fabrics,
    IReadOnlyList<FabricValuationShortageResponse> Shortages,
    DateTimeOffset CalculatedAt,
    string PriceSource);
