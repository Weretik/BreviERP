namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;

/// <summary>A row of «Рух тканини».</summary>
/// <param name="Kind">Opening, Receipt, Issue, Adjustment or Reversal.</param>
/// <param name="FabricCode">The code of the line moved; <c>GroupCode</c> is the fabric the stock shows.</param>
/// <param name="MetersAfter">Balance of that line after the movement; the pair total is on «Залишки».</param>
/// <param name="PairMetersAfter">«Залишок пари»: the merged pair (all codes of the fabric, one colour) after the
/// movement in the ledger order; null where the row is not from «Рух тканини».</param>
public sealed record FabricMovementRowResponse(
    int Id,
    int StockId,
    string Kind,
    DateOnly OperationDate,
    DateTimeOffset CreatedAt,
    string FabricCode,
    string GroupCode,
    string FabricName,
    string Color,
    string? SupplierName,
    string? Document,
    int? OrderId,
    int? ItemId,
    int? OrderNumber,
    string? ModelName,
    string? Destination,
    decimal Delta,
    decimal MetersAfter,
    string? Note,
    string CreatedByName,
    bool IsReversed,
    int? ReversesMovementId,
    bool CanReverse,
    bool IsVoided,
    int Revision,
    bool CanEdit,
    decimal? PerProduct = null,
    decimal? PairMetersAfter = null);
