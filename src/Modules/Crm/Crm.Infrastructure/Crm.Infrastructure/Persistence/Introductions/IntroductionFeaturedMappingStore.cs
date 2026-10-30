using System.Data;
using Crm.Application.Quotes.Introductions;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Introductions;

public sealed class IntroductionFeaturedMappingStore(CrmDbContext db) : IIntroductionFeaturedMappingStore
{
    public async Task<FeaturedModelMapping?> ReadActiveAsync(CancellationToken cancellationToken)
    {
        var activeId = await db.IntroductionFeaturedMappingStates.AsNoTracking()
            .Where(x => x.Id == 1).Select(x => x.ActiveVersionId)
            .SingleOrDefaultAsync(cancellationToken);
        if (activeId is not > 0) return null;
        var version = await db.IntroductionFeaturedMappingVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == activeId, cancellationToken);
        return version?.ToMapping();
    }

    public async Task<FeaturedModelMapping> ActivateAsync(IntroductionFeaturedMappingVersion version,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(4849701693292117)", cancellationToken);
        var state = await db.IntroductionFeaturedMappingStates.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (state?.ActiveVersionId is > 0)
        {
            var active = await db.IntroductionFeaturedMappingVersions.AsNoTracking()
                .SingleAsync(x => x.Id == state.ActiveVersionId, cancellationToken);
            if (active.SourceSnapshotHash == version.SourceSnapshotHash &&
                active.SummerModelCode == version.SummerModelCode &&
                active.WinterModelCode == version.WinterModelCode &&
                active.SummerProductId == version.SummerProductId &&
                active.WinterProductId == version.WinterProductId)
            {
                await transaction.CommitAsync(cancellationToken);
                return active.ToMapping();
            }
        }
        if (state is null)
        {
            state = IntroductionFeaturedMappingState.Create();
            db.IntroductionFeaturedMappingStates.Add(state);
            await db.SaveChangesAsync(cancellationToken);
        }
        db.IntroductionFeaturedMappingVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);
        state.Activate(version.Id);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return version.ToMapping();
    }
}
