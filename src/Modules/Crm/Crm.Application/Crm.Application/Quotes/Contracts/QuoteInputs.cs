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
/// <param name="Kind">`Sewing`, `Ppe` or `OneOffPpe`.</param>
/// <param name="Name">Required for a one-off PPE; ignored for Catalog items.</param>
public sealed record QuoteItemInput(
    long? Id,
    string Kind,
    int? ProductId,
    string? Name,
    string? Description,
    string? PhotoUrl,
    string? SupplierUrl,
    IReadOnlyList<QuoteOptionInput> Options);

/// <param name="Id">The saved row, or null for a new one.</param>
/// <param name="ArticleId">The fabric variant of a sewing model.</param>
/// <param name="ManualReason">Required when a PPE price differs from the Catalog one.</param>
public sealed record QuoteOptionInput(
    long? Id,
    long? ArticleId,
    bool IsRecommended,
    QuotePriceInput Retail,
    QuotePriceInput Wholesale,
    QuotePriceInput Large,
    string? ManualReason);

/// <param name="Manual">A manual price (PPE, one-off PPE); null keeps the Catalog price.</param>
public sealed record QuotePriceInput(bool Shown, decimal? Manual);
