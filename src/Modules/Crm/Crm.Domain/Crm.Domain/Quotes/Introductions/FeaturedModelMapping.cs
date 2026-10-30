namespace Crm.Domain.Quotes.Introductions;

/// <summary>Approved, versioned Catalog identity mapping; names are never used to infer a featured model.</summary>
public sealed record FeaturedModelMapping(Guid VersionId, int SummerProductId, int WinterProductId)
{
    public int For(IntroductionSeason season) => season switch
    {
        IntroductionSeason.Summer => SummerProductId,
        IntroductionSeason.Winter => WinterProductId,
        _ => throw new IntroductionValidationException("season_invalid")
    };
}
