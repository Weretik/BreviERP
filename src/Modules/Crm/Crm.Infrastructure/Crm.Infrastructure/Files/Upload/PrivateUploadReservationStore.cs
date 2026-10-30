using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.Files.Upload;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Upload;

internal sealed class PrivateUploadReservationStore(CrmDbContext db, TimeProvider time, PrivateUploadOptions options)
{
    internal Task<bool> AccessibleAsync(Guid draftId, CrmActor actor, CancellationToken ct)
        => db.CustomDrafts.Where(CustomAccess.For(actor)).AnyAsync(x => x.PublicId == draftId, ct);

    internal async Task<Result<CustomPrivateFile>> ReserveAsync(UploadPrivateFileCommand command, ValidatedUploadBody body, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var key = BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes($"upload:{command.Actor.UserId}:{command.RequestId:N}")), 0);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", ct);
            var draft = await new CustomWriteSession(db, command.Actor).FindDraftAsync(command.DraftId, command.Actor, ct);
            if (draft is null) return Result.NotFound();
            if (draft.Quote?.Archived == true) return Result.Conflict("quote_not_editable");
            var existing = await db.Set<CustomPrivateFile>().SingleOrDefaultAsync(x =>
                x.ActorId == new CrmUserId(command.Actor.UserId) && x.RequestId == command.RequestId, ct);
            if (existing is not null)
            {
                if (existing.DraftId != draft.Id || existing.IntentHash != body.IntentHash) return Result.Conflict("request_id_payload_conflict");
                var retained = await db.Set<CustomDraftAttachment>().AnyAsync(x => x.DraftId == existing.DraftId && x.FileId == existing.Id, ct)
                    || await db.Set<CustomVersionAttachment>().AnyAsync(x => x.DraftId == existing.DraftId && x.FileId == existing.Id, ct);
                return existing.State is CustomFileState.Deleting or CustomFileState.Expired
                    || (existing.ExpiresAt <= time.GetUtcNow() && !retained)
                    ? Result.Conflict("file_upload_expired") : Result.Success(existing);
            }
            if (await db.Set<CustomPrivateFile>().CountAsync(x => x.DraftId == draft.Id && x.State != CustomFileState.Expired, ct) >= 30)
                return Result.Invalid(new ValidationError("file", "private_file_limit"));
            var at = time.GetUtcNow();
            var file = CustomPrivateFile.Reserve(draft.Id, new CrmUserId(command.Actor.UserId), command.RequestId,
                body.IntentHash, body.ContentType, body.Bytes.Length, body.Sha256, at, at.Add(options.TemporaryLifetime));
            db.Set<CustomPrivateFile>().Add(file);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success(file);
        }
        finally { db.ChangeTracker.Clear(); }
    }

    internal async Task<Result<CustomPrivateFile>> CompleteAsync(Guid draftId, Guid fileId, CrmActor actor, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var draft = await new CustomWriteSession(db, actor).FindDraftAsync(draftId, actor, ct);
            if (draft is null) return Result.NotFound();
            if (draft.Quote?.Archived == true) return Result.Conflict("quote_not_editable");
            var file = await db.Set<CustomPrivateFile>().FromSqlInterpolated(
                $"""SELECT * FROM crm."CustomPrivateFiles" WHERE "PublicId" = {fileId} FOR UPDATE""")
                .SingleOrDefaultAsync(x => x.DraftId == draft.Id && x.ActorId == new CrmUserId(actor.UserId), ct);
            if (file is null) return Result.NotFound();
            if (file.State == CustomFileState.Uploaded) return Result.Success(file);
            file.Complete(time.GetUtcNow());
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success(file);
        }
        catch (CustomSewingValidationException) { return Result.Conflict("file_upload_expired_or_changed"); }
        finally { db.ChangeTracker.Clear(); }
    }
}
