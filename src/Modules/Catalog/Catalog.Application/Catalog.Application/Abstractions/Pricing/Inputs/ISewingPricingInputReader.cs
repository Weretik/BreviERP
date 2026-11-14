using Catalog.Application.Abstractions.Pricing.Calculations;

namespace Catalog.Application.Abstractions.Pricing.Inputs;

public interface ISewingPricingInputReader
{
    Task<SewingArticlePricingSnapshot?> ReadAsync(int productId, long articleId, CancellationToken cancellationToken);
}

