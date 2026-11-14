namespace Warehouse.Application.Features.Stock.Shared;

/// <param name="Stock">Physical balance of the pair; may be negative.</param>
/// <param name="Need">What the plans still need.</param>
/// <param name="Shortage">The negative balance already there, as a positive figure.</param>
/// <param name="Supplier">The supplier of the fabric in «Тканини», for «Що замовити».</param>
/// <param name="NeedBy">«Потрібно до»: the nearest due date of the tasks still short of this pair, or else of those
/// that need it; null when none has a date.</param>
public sealed record FabricBookingPairResponse(
    string FabricCode,
    string FabricName,
    string Color,
    decimal Stock,
    decimal Need,
    decimal Reserved,
    decimal Free,
    decimal ToOrder,
    decimal Shortage,
    IReadOnlyList<FabricBookingItemResponse> Items,
    string? Supplier = null,
    DateOnly? NeedBy = null);
