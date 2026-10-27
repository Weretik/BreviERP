namespace Crm.Application.Quotes.Pricing;

public interface IQuoteCatalogPriceReader
{
    Task<QuoteCatalogPrice> ReadAsync(string kind, int productId, long? articleId, CancellationToken cancellationToken);
}
