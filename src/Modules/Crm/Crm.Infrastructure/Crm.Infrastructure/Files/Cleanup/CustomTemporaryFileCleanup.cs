using Crm.Application.Files.Cleanup;
using Crm.Application.Files.Storage;
using Crm.Infrastructure.Persistence;

namespace Crm.Infrastructure.Files.Cleanup;

public sealed class CustomTemporaryFileCleanup(CrmDbContext db, IReservedPrivateFileStore bytes, TimeProvider? clock = null)
    : ICustomTemporaryFileCleanup
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    private readonly TemporaryFileCleanupStore metadata = new(db);

    public async Task<int> CleanupAsync(int maximumFiles, CancellationToken ct)
    {
        if (maximumFiles is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(maximumFiles));
        var deleted = 0;
        for (var index = 0; index < maximumFiles; index++)
        {
            var claim = await metadata.ClaimAsync(time.GetUtcNow(), ct);
            if (claim is null) break;
            if (await metadata.ReferencedAsync(claim.Id, claim.DraftId, ct)) continue;
            var removed = await bytes.DeleteTemporaryAsync(claim.Root, claim.File, ct);
            if (!removed.IsSuccess) break;
            if (await metadata.CompleteAsync(claim, ct)) deleted++;
        }
        return deleted;
    }
}
