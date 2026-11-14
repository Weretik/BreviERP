namespace Catalog.Application.Features.Product.Queries.GetProductOverviewPage;

/// <summary>
/// One row of the «Товари» page with its prices (CAT-013). <paramref name="Prices"/> is the lowest price of each tier
/// among the active fabric variants (sewing) or the rule price (PPE); null when there is none.
/// <paramref name="Problems"/> are the reasons a price is missing (of the row, or of an active variant); empty when
/// everything is priced. <paramref name="MainPhotoUrl"/> is the preview of the main visible photo (CAT-016);
/// <paramref name="MainPhotoFullUrl"/> is the same photo in full size, for КП and quick view (CAT-019).
/// </summary>
public sealed record ProductOverviewItem(
    int Id,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    DateTimeOffset UpdatedAtUtc,
    ProductPriceTiers? Prices,
    DateTimeOffset? PricesUpdatedAt,
    IReadOnlyList<string> Problems,
    IReadOnlyList<ProductOverviewVariant> Variants,
    string? MainPhotoUrl = null,
    string? MainPhotoFullUrl = null);

/// <summary>
/// A fabric variant of a sewing model (an Article): its fabric and current published prices. PPE rows have none.
/// <paramref name="Problems"/> are the calculation codes when an active variant has no price.
/// </summary>
public sealed record ProductOverviewVariant(
    long ArticleId,
    string SaleSku,
    bool Active,
    int? FabricId,
    string? FabricName,
    ProductPriceTiers? Prices,
    DateTimeOffset? PublishedAt,
    IReadOnlyList<string> Problems);

/// <summary>Prices for 1–9, 10–40 and from 41 pieces (`SewingPriceTiers.ForQuantity`).</summary>
public sealed record ProductPriceTiers(decimal Retail, decimal Wholesale, decimal LargeWholesale);
