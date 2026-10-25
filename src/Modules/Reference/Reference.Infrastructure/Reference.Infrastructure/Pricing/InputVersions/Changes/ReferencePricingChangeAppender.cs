using System.Security.Cryptography;
using Reference.Application.Contracts.Pricing;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

internal sealed class ReferencePricingChangeAppender(ReferenceDbContext db)
{
    public bool HasChanges() => TrackedReferencePricingChanges.Any(db.ChangeTracker);

    public async Task AppendAsync(CancellationToken cancellationToken)
    {
        var versions = db.Set<ReferencePricingInputVersion>();
        foreach (var change in TrackedReferencePricingChanges.Read(db.ChangeTracker))
        {
            var latest = await versions.Where(change.VersionFilter).AsNoTracking()
                .OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
            if (latest is null)
                continue;
            var at = DateTimeOffset.UtcNow;
            var expectedOriginal = latest.OperationId.HasValue ? latest.Value ?? 0 : MoneyAmount.From(latest.Value ?? 0).Value;
            if (latest.EffectiveFrom >= at || expectedOriginal != change.OriginalValue)
                throw new ReferencePricingConflictException("Pricing input changed or has a future revision. Reload before editing.");
            var hash = Convert.ToHexString(SHA256.HashData(Guid.NewGuid().ToByteArray()));
            versions.Add(ReferencePricingInputVersion.Create(latest.FabricId, latest.AccessoryId, latest.OperationId,
                latest.OtherCostCode, change.Value, latest.Unit, latest.IsActive, true, latest.Revision + 1, at,
                latest.SourceCode, hash, at.ToString("O")));
        }
    }
}
