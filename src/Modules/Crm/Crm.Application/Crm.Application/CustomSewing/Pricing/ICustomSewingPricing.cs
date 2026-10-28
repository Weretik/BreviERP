using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Application.CustomSewing.Pricing;

public interface ICustomSewingPricing
{
    Task<CustomPriceBasis> CalculateAsync(CustomConfiguration configuration, DateTimeOffset at,
        CancellationToken cancellationToken);

    Task<CustomPriceBasis> CalculateAsync(CustomConfiguration configuration, DateTimeOffset at,
        CancellationToken cancellationToken, bool artworkVerified)
        => CalculateAsync(configuration, at, cancellationToken);
}
