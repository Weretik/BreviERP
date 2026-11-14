namespace Warehouse.Application.Features.Stock.Shared;

/// <param name="Layer">Upper, Lining, Insulation or Fleece.</param>
/// <param name="Covered">What issues covered of the plan.</param>
/// <param name="Remaining">Plan − covered, not below zero.</param>
public sealed record FabricBookingItemResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    DateOnly? DueDate,
    string Layer,
    decimal Plan,
    decimal Covered,
    decimal Remaining,
    decimal Reserved,
    decimal ToOrder);
