using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public sealed class IntroductionFeaturedMappingService(
    IIntroductionSourceMappingReader sources, IIntroductionFeaturedMappingStore store)
{
    public async Task<Result<FeaturedModelMapping>> ActivateAsync(CrmActor actor,
        string snapshotHash, string summerModelCode, string winterModelCode,
        CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || !actor.IsAdmin)
            return Result<FeaturedModelMapping>.Forbidden();
        if (string.IsNullOrWhiteSpace(snapshotHash) || string.IsNullOrWhiteSpace(summerModelCode) ||
            string.IsNullOrWhiteSpace(winterModelCode))
            return Result<FeaturedModelMapping>.Invalid(new ValidationError("mapping", "featured_mapping_invalid"));
        var mapping = await sources.ResolveAsync(snapshotHash, summerModelCode, winterModelCode, cancellationToken);
        if (mapping is null)
            return Result<FeaturedModelMapping>.Invalid(new ValidationError("mapping", "featured_source_unavailable"));
        try
        {
            var version = IntroductionFeaturedMappingVersion.Create(snapshotHash, summerModelCode,
                winterModelCode, mapping.SummerProductId, mapping.WinterProductId,
                new CrmUserId(actor.UserId));
            return Result.Success(await store.ActivateAsync(version, cancellationToken));
        }
        catch (IntroductionValidationException exception)
        {
            return Result<FeaturedModelMapping>.Invalid(new ValidationError("mapping", exception.Message));
        }
    }
}
