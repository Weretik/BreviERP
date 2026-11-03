namespace Catalog.Application.Features.Product.Overview.Prices.DTOs;

/// <summary>
/// The price state of one «Товари» row. <paramref name="Status"/>: Ready (all priced), Stale (a published price
/// differs from the live one), Unpublished (a calculable Article was never published), Blocked (no price can be
/// calculated; <paramref name="Blockers"/> say why). <paramref name="From"/> is the lowest price of each tier.
/// </summary>
public sealed record ProductOverviewPrices(
    int ProductId,
    string Status,
    ProductPriceTiers? From,
    DateTimeOffset? PricesUpdatedAt,
    IReadOnlyList<string> Blockers,
    IReadOnlyList<ProductOverviewArticlePrices> Articles);

/// <summary>
/// One sewing Article: the published (fixed) price, the live price from today's inputs, and whether they differ.
/// <paramref name="FabricId"/> is the Reference fabric that names the Article in the price list.
/// </summary>
public sealed record ProductOverviewArticlePrices(
    long ArticleId,
    string SaleSku,
    bool Active,
    int? FabricId,
    ProductPriceTiers? Published,
    DateTimeOffset? PublishedAt,
    ProductPriceTiers? Live,
    bool IsStale,
    IReadOnlyList<string> Blockers);

/// <summary>Prices for 1–9, 10–40 and from 41 pieces (`SewingPriceTiers.ForQuantity`).</summary>
public sealed record ProductPriceTiers(decimal Retail, decimal Wholesale, decimal LargeWholesale);
