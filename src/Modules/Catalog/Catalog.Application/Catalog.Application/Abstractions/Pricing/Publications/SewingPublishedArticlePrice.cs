using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Abstractions.Pricing.Publications;

public sealed record SewingPublishedArticlePrice(
    long ArticleId, string SaleSku, Guid PublicationId, Guid CalculationId,
    DateTimeOffset PublishedAt, SewingPriceTiers Prices);
