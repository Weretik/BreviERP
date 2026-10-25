namespace Catalog.Application.Contracts.Pricing;

public interface ISewingPricingInputReader
{
    Task<SewingArticlePricingSnapshot?> ReadAsync(int productId, long articleId, CancellationToken cancellationToken);
}

