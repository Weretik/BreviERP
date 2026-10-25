using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Contracts.Pricing;

public sealed record SewingPublishedArticlePrice(
    long ArticleId, string SaleSku, Guid PublicationId, Guid CalculationId,
    DateTimeOffset PublishedAt, SewingPriceTiers Prices);
