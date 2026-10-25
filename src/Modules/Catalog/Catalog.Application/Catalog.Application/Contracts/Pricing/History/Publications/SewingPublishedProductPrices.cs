namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingPublishedProductPrices(
    int ProductId, int ArticleCount, IReadOnlyList<SewingPublishedArticlePrice> Articles);
