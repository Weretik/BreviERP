namespace Catalog.Application.Abstractions.Pricing;

public sealed record SewingPublishedProductPrices(
    int ProductId, int ArticleCount, IReadOnlyList<SewingPublishedArticlePrice> Articles);
