using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Finalize;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Infrastructure.Files.Verification;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Finalize;

public sealed class FinalizedCustomFilesWriter(CrmDbContext db, IPrivateFileStore bytes, TimeProvider? clock = null)
    : IFinalizedCustomFilesWriter
{
    public async Task ReplaceAsync(CustomSewingDraft draft, IReadOnlyList<CustomFileSelection> selection,
        CrmActor actor, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("File finalization requires the root transaction.");
        var time = clock ?? TimeProvider.System;
        var ids = selection.Select(x => x.FileId).ToArray();
        var verified = await new CustomFileAvailability(db, bytes, time).VerifyAsync(draft.PublicId, ids, actor, false, ct);
        if (!verified.IsSuccess)
            throw new CustomSewingValidationException(verified.Status == Ardalis.Result.ResultStatus.Error
                ? "private_file_storage_unavailable" : "private_file_not_finalized");
        var files = ids.Length == 0 ? [] : await db.Set<CustomPrivateFile>().FromSqlInterpolated(
            $"""SELECT * FROM crm."CustomPrivateFiles" WHERE "DraftId" = {draft.Id.Value} AND "PublicId" = ANY({ids}) ORDER BY "Id" FOR UPDATE""")
            .ToArrayAsync(ct);
        var retained = await db.Set<CustomDraftAttachment>().Where(x => x.DraftId == draft.Id).Select(x => x.File.PublicId)
            .Concat(db.Set<CustomVersionAttachment>().Where(x => x.DraftId == draft.Id).Select(x => x.File.PublicId))
            .Distinct().ToArrayAsync(ct);
        if (files.Length != ids.Length || files.Any(x => x.State != CustomFileState.Uploaded
            || (x.ExpiresAt <= time.GetUtcNow() && !retained.Contains(x.PublicId))))
            throw new CustomSewingValidationException("private_file_not_finalized");
        await db.Set<CustomDraftAttachment>().Where(x => x.DraftId == draft.Id).ExecuteDeleteAsync(ct);
        var byId = files.ToDictionary(x => x.PublicId);
        db.Set<CustomDraftAttachment>().AddRange(selection.Select(x =>
            CustomDraftAttachment.Create(draft.Id, byId[x.FileId].Id, x.ShowInQuote, x.Production)));
    }
}
