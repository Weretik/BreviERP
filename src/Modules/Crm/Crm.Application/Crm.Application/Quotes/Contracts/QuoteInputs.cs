namespace Crm.Application.Quotes.Contracts;

/// <summary>
/// The whole editable content of a КП. Catalog prices are never taken from the client: a new row gets them from the
/// Catalog, an existing row keeps its own. Only PPE rows carry manual prices.
/// </summary>
/// <param name="Version">The version the editor loaded; ignored on create.</param>
public sealed record SaveQuoteInput(
    int Version,
    string InternalName,
    long? CustomerId,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote,
    IReadOnlyList<QuoteItemInput> Items);

/// <param name="Id">The saved item, or null for a new one.</param>
/// <param name="Kind">`Sewing`, `Ppe`, `OneOffPpe`, `CustomSewing` or `Embroidery` (a saved one only: new ones come from «Додати в КП»).</param>
/// <param name="ProductId">The Catalog product; for a «Виріб з нуля» the model whose composition was changed, or null.</param>
/// <param name="Name">Required for a one-off PPE and a «Виріб з нуля»; ignored for Catalog items.</param>
/// <param name="Lines">The composition of a «Виріб з нуля»; null for other items.</param>
public sealed record QuoteItemInput(
    long? Id,
    string Kind,
    int? ProductId,
    string? Name,
    string? Description,
    string? PhotoUrl,
    string? SupplierUrl,
    IReadOnlyList<QuoteOptionInput> Options,
    IReadOnlyList<QuoteItemLineInput>? Lines = null);

/// <param name="Kind">`Accessory`, `Operation`, `Service` or `Fabric` (an extra shared fabric).</param>
/// <param name="ReferenceId">Reference garment accessory, operation or fabric; 0 for a service.</param>
/// <param name="ManualMinutes">Minutes of an operation without a fixed norm.</param>
/// <param name="CostCode">The Reference other-cost code of a service.</param>
public sealed record QuoteItemLineInput(
    string Kind, int ReferenceId, decimal Quantity, decimal? ManualMinutes, string? CostCode = null);

/// <param name="Id">The saved row, or null for a new one.</param>
/// <param name="ArticleId">The fabric variant of a sewing model.</param>
/// <param name="ManualReason">Required when a PPE price differs from the Catalog one.</param>
/// <param name="Fabrics">
/// The fabrics of a «Виріб з нуля» variant, summed in its price (R-26), or the one fabric of a model row in another
/// fabric (R-27); null for a Catalog variant and a PPE.
/// </param>
public sealed record QuoteOptionInput(
    long? Id,
    long? ArticleId,
    bool IsRecommended,
    QuotePriceInput Retail,
    QuotePriceInput Wholesale,
    QuotePriceInput Large,
    string? ManualReason,
    IReadOnlyList<QuoteOptionFabricInput>? Fabrics = null);

/// <param name="FabricId">Reference fabric.</param>
/// <param name="Meters">Metres of that fabric in one product.</param>
public sealed record QuoteOptionFabricInput(int FabricId, decimal Meters);

/// <param name="Manual">A manual price (PPE, one-off PPE); null keeps the Catalog price.</param>
public sealed record QuotePriceInput(bool Shown, decimal? Manual);
