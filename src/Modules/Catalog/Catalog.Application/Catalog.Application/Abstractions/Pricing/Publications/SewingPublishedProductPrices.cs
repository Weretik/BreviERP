namespace Catalog.Application.Abstractions.Pricing.Publications;

public sealed record SewingPublishedProductPrices(
    int ProductId, int ArticleCount, IReadOnlyList<SewingPublishedArticlePrice> Articles);
