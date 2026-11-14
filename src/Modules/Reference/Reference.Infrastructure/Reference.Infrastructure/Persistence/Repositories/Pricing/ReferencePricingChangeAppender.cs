using System.Security.Cryptography;
using Reference.Domain.Pricing;
using Reference.Application.Abstractions;
using Reference.Infrastructure.Persistence;
using Reference.Application.Abstractions.Pricing;


namespace Reference.Infrastructure.Persistence.Repositories.Pricing;

internal sealed class ReferencePricingChangeAppender(ReferenceDbContext db)
{
    private const string SourcePrefix = "reference:";

    public bool HasChanges() => TrackedReferencePricingChanges.Any(db.ChangeTracker);

    public async Task AppendAsync(CancellationToken cancellationToken)
    {
        var versions = db.Set<ReferencePricingInputVersion>();
        var at = DateTimeOffset.UtcNow;
        foreach (var change in TrackedReferencePricingChanges.Read(db.ChangeTracker))
        {
            if (change.Deleted)
            {
                // The row goes; its version history is only meaningful while it exists.
                // Versions are append-only, so a tracked stored one is never saved: EF would null its FK instead.
                foreach (var tracked in db.ChangeTracker.Entries<ReferencePricingInputVersion>()
                             .Where(x => x.State != EntityState.Added).ToList())
                    tracked.State = EntityState.Detached;
                await versions.Where(change.VersionFilter).ExecuteDeleteAsync(cancellationToken);
                continue;
            }

            var latest = await versions.Where(change.VersionFilter).AsNoTracking()
                .OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
            if (latest is null)
            {
                versions.Add(Version(change, 1, SourcePrefix + change.Key, at));
                continue;
            }

            var current = change.OperationId.HasValue ? latest.Value ?? 0 : MoneyAmount.From(latest.Value ?? 0).Value;
            if (current == change.Value && latest.Unit == change.Unit && latest.IsActive == change.IsActive)
                continue;
            if (latest.EffectiveFrom >= at
                || (change.OriginalValue.HasValue && change.OriginalValue != change.Value && current != change.OriginalValue))
                throw new ReferencePricingConflictException("Ціну вже змінили в іншому вікні або на неї заплановано нову. Оновіть сторінку і внесіть зміну ще раз.");
            versions.Add(Version(change, latest.Revision + 1, latest.SourceCode, at));
        }
    }

    private static ReferencePricingInputVersion Version(
        ReferencePricingTrackedChange change, int revision, string sourceCode, DateTimeOffset at) =>
        ReferencePricingInputVersion.Create(change.FabricId, change.AccessoryId, change.OperationId, null,
            change.Value, change.Unit, change.IsActive, true, revision, at, sourceCode,
            Convert.ToHexString(SHA256.HashData(Guid.NewGuid().ToByteArray())), at.ToString("O"));
}
