namespace Catalog.Application.Abstractions.Pricing;

public interface ISewingPricingSettingsReader
{
    Task<IReadOnlyList<SewingPricingSettingsSelection>> ReadCandidatesAsync(DateTimeOffset at, CancellationToken cancellationToken);
}

