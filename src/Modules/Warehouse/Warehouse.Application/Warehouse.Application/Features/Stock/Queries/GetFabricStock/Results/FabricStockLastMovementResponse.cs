namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;

/// <summary>The latest movement of a pair for «Остання операція».</summary>
/// <param name="Kind">Opening, Receipt, Issue, Adjustment or Reversal; an issue with an order number went to a task.</param>
public sealed record FabricStockLastMovementResponse(
    int Id,
    string Kind,
    DateOnly OperationDate,
    decimal Delta,
    int? OrderNumber,
    string? Document,
    string? SupplierName,
    string? Destination);
