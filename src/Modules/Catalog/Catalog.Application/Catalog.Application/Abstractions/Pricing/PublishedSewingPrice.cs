using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Abstractions.Pricing;

public sealed record PublishedSewingPrice(
    Guid Id, Guid CalculationId, int ProductId, long ArticleId,
    DateTimeOffset PublishedAt, string PublishedBy, SewingPriceTiers Prices);
