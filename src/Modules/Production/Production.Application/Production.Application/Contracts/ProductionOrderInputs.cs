namespace Production.Application.Contracts;

/// <param name="Kind">Embroidery or Print.</param>
/// <param name="EmbroideryCalculationId">A saved «Ціна вишивки» calculation; only for an embroidery.</param>
public sealed record ItemApplicationInput(string Kind, string? Place, string? Description, string? Color,
    long? EmbroideryCalculationId = null);

/// <summary>What the manager or the head of the workshop fill in for an item.</summary>
public sealed record ProductionItemDetailsInput(
    int Quantity,
    DateOnly DueDate,
    string MainColor,
    IReadOnlyList<string> AdditionalColors,
    string? SizesText,
    string? TechnicalChanges,
    string? Comment,
    IReadOnlyList<ItemApplicationInput> Applications);

/// <summary>An item of the КП to sew: the КП position and its fabric variant.</summary>
public sealed record CreateProductionItemInput(long QuoteItemId, long QuoteOptionId, ProductionItemDetailsInput Details);

/// <summary>«Передати в цех». <paramref name="RequestId"/> is new for every form; a repeat returns the same order.</summary>
/// <param name="PriceTier">Retail, Wholesale or Large: the КП price kept for the owner's reports.</param>
public sealed record CreateProductionOrderInput(
    Guid RequestId,
    long QuoteId,
    string ClientName,
    DateOnly OrderDate,
    string? Comment,
    string PriceTier,
    IReadOnlyList<CreateProductionItemInput> Items);

/// <param name="SewingRate">Applied only for Admin and the head of the workshop; null clears it.</param>
public sealed record UpdateProductionItemInput(int Id, ProductionItemDetailsInput Details, decimal? SewingRate);

public sealed record UpdateProductionOrderInput(
    int Version,
    string ClientName,
    DateOnly OrderDate,
    string? Comment,
    IReadOnlyList<UpdateProductionItemInput> Items);

public sealed record CopyProductionOrderInput(Guid RequestId);

/// <summary>An order of the old «Цех» from the transfer file.</summary>
/// <param name="Status">New, Closed or Cancelled.</param>
public sealed record ProductionOrderImportRow(
    string LegacyId,
    int Number,
    string? QuoteNumber,
    string ClientName,
    string? ManagerEmail,
    string ManagerName,
    string Status,
    DateOnly OrderDate,
    string? Comment,
    IReadOnlyList<ProductionItemImportRow> Items);

public sealed record ProductionItemImportRow(
    string? ModelCode,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    decimal? SewingRate,
    ProductionItemDetailsInput Details);
