
namespace Catalog.Infrastructure.Persistence.Repositories.Pricing.Publications;

internal sealed record SewingPublishedArticlePriceRow(
    int ProductId, long ArticleId, string SaleSku, Guid PublicationId, Guid CalculationId,
    DateTimeOffset PublishedAt, decimal Retail, decimal Wholesale, decimal LargeWholesale);
