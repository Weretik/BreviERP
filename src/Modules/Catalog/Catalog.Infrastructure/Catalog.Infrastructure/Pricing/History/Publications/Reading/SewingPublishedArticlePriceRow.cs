namespace Catalog.Infrastructure.Pricing.History.Publications.Reading;

internal sealed record SewingPublishedArticlePriceRow(
    int ProductId, long ArticleId, string SaleSku, Guid PublicationId, Guid CalculationId,
    DateTimeOffset PublishedAt, decimal Retail, decimal Wholesale, decimal LargeWholesale);
