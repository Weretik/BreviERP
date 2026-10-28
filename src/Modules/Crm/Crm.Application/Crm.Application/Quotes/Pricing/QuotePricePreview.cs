namespace Crm.Application.Quotes.Pricing;

public sealed record QuotePricePreview(
    Guid BasisToken,
    DateTimeOffset ExpiresAt,
    QuotePriceIntent Intent,
    QuoteCatalogPrice? Source,
    decimal? Retail,
    decimal? Wholesale,
    decimal? LargeWholesale,
    IReadOnlyList<string> Blockers,
    string Currency,
    string TaxBasis,
    QuoteCustomVersionSnapshot? CustomVersion = null);
