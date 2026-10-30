using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public interface IIntroductionFeaturedMappingStore
{
    Task<FeaturedModelMapping?> ReadActiveAsync(CancellationToken cancellationToken);
    Task<FeaturedModelMapping> ActivateAsync(IntroductionFeaturedMappingVersion version,
        CancellationToken cancellationToken);
}
