using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Abstractions.Pricing;

public interface ISewingPricingSettingsWriter
{
    Task<Result<SewingPricingSettingsSelection>> AppendAsync(
        long expectedVersionId, Guid requestId, SewingCalculationSettings settings, string actor,
        CancellationToken cancellationToken);
}
