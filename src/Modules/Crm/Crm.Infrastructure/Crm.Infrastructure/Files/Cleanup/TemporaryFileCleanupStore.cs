using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Cleanup;

internal sealed class TemporaryFileCleanupStore(CrmDbContext db)
{
    internal async Task<TemporaryFileCleanupClaim?> ClaimAsync(DateTimeOffset now, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var files = await db.Set<CustomPrivateFile>().FromSqlInterpolated($"""
                SELECT f.* FROM crm."CustomPrivateFiles" f
                WHERE f."State" IN ('Reserved','Uploaded','Deleting') AND f."ExpiresAt" <= {now}
                  AND NOT EXISTS (SELECT 1 FROM crm."CustomDraftAttachments" d WHERE d."DraftId"=f."DraftId" AND d."FileId"=f."Id")
                  AND NOT EXISTS (SELECT 1 FROM crm."CustomVersionAttachments" v WHERE v."DraftId"=f."DraftId" AND v."FileId"=f."Id")
                ORDER BY f."ExpiresAt",f."Id" LIMIT 1 FOR UPDATE SKIP LOCKED
                """).ToArrayAsync(ct);
            var file = files.SingleOrDefault();
            if (file is null) return null;
            // The locking SELECT can have a snapshot older than a just-committed finalization.
            // Recheck in a fresh statement while holding the file row before changing its state.
            if (await ReferencedAsync(file.Id, file.DraftId, ct)) return null;
            file.BeginCleanup(now);
            var root = await db.CustomDrafts.Where(x => x.Id == file.DraftId).Select(x => x.PublicId).SingleAsync(ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new TemporaryFileCleanupClaim(file.Id, file.DraftId, root, file.PublicId);
        }
        finally { db.ChangeTracker.Clear(); }
    }

    internal async Task<bool> ReferencedAsync(CustomFileId fileId, Crm.Domain.CustomSewing.Identifiers.CustomDraftId draftId, CancellationToken ct)
        => await db.Set<CustomDraftAttachment>().AnyAsync(x => x.DraftId == draftId && x.FileId == fileId, ct)
            || await db.Set<CustomVersionAttachment>().AnyAsync(x => x.DraftId == draftId && x.FileId == fileId, ct);

    internal async Task<bool> CompleteAsync(TemporaryFileCleanupClaim claim, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var file = await db.Set<CustomPrivateFile>().FromSqlInterpolated(
                $"""SELECT * FROM crm."CustomPrivateFiles" WHERE "PublicId" = {claim.File} FOR UPDATE""").SingleAsync(ct);
            if (file.State != CustomFileState.Deleting || await ReferencedAsync(file.Id, file.DraftId, ct)) return false;
            file.FinishCleanup();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return true;
        }
        finally { db.ChangeTracker.Clear(); }
    }

}
