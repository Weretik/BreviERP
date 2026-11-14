namespace Catalog.Application.Abstractions.Pricing.Settings;

public interface ISewingPricingSettingsReader
{
    Task<IReadOnlyList<SewingPricingSettingsSelection>> ReadCandidatesAsync(DateTimeOffset at, CancellationToken cancellationToken);
}

