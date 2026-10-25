namespace Catalog.Application.Contracts.Pricing;

public interface ISewingPricingSettingsReader
{
    Task<IReadOnlyList<SewingPricingSettingsSelection>> ReadCandidatesAsync(DateTimeOffset at, CancellationToken cancellationToken);
}

