using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Abstractions.Pricing.Settings;

public interface ISewingPricingSettingsWriter
{
    Task<Result<SewingPricingSettingsSelection>> AppendAsync(
        long expectedVersionId, Guid requestId, SewingCalculationSettings settings, string actor,
        CancellationToken cancellationToken);
}
