namespace Catalog.Application.Abstractions.Pricing;

public interface ISewingPricingInputReader
{
    Task<SewingArticlePricingSnapshot?> ReadAsync(int productId, long articleId, CancellationToken cancellationToken);
}

