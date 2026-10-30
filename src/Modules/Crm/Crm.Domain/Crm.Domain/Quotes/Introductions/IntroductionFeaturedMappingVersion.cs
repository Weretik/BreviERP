using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Quotes.Introductions;

public sealed class IntroductionFeaturedMappingVersion : BaseEntity<long>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public string SourceSnapshotHash { get; private set; } = null!;
    public string SummerModelCode { get; private set; } = null!;
    public string WinterModelCode { get; private set; } = null!;
    public int SummerProductId { get; private set; }
    public int WinterProductId { get; private set; }
    public CrmUserId ApprovedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private IntroductionFeaturedMappingVersion() { }

    public static IntroductionFeaturedMappingVersion Create(string sourceSnapshotHash,
        string summerModelCode, string winterModelCode, int summerProductId, int winterProductId,
        CrmUserId approvedBy)
    {
        if (sourceSnapshotHash is null || sourceSnapshotHash.Length != 64 || !sourceSnapshotHash.All(Uri.IsHexDigit) ||
            string.IsNullOrWhiteSpace(summerModelCode) || summerModelCode.Length > 80 ||
            string.IsNullOrWhiteSpace(winterModelCode) || winterModelCode.Length > 80 ||
            summerProductId <= 0 || winterProductId <= 0 || summerProductId == winterProductId ||
            approvedBy.Value <= 0)
            throw new IntroductionValidationException("featured_mapping_invalid");
        return new IntroductionFeaturedMappingVersion
        {
            SourceSnapshotHash = sourceSnapshotHash.ToLowerInvariant(),
            SummerModelCode = summerModelCode.Trim(), WinterModelCode = winterModelCode.Trim(),
            SummerProductId = summerProductId, WinterProductId = winterProductId,
            ApprovedBy = approvedBy
        };
    }

    public FeaturedModelMapping ToMapping() => new(PublicId, SummerProductId, WinterProductId);
}
