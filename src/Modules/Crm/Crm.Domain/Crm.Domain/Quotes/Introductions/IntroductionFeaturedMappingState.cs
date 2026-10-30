using BuildingBlocks.Domain.Entity;

namespace Crm.Domain.Quotes.Introductions;

public sealed class IntroductionFeaturedMappingState : BaseEntity<int>
{
    public long? ActiveVersionId { get; private set; }
    public long Revision { get; private set; } = 1;
    private IntroductionFeaturedMappingState() { }
    public static IntroductionFeaturedMappingState Create() => new() { Id = 1 };
    public void Activate(long versionId)
    {
        if (versionId <= 0) throw new IntroductionValidationException("featured_mapping_invalid");
        ActiveVersionId = versionId;
        Revision++;
    }
}
