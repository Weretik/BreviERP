using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.Files.Verification;
using Crm.Application.Files.Verification.Specifications;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Verification;

internal sealed class CustomFileAvailability(CrmDbContext db, IPrivateFileStore bytes, TimeProvider time)
{
    internal async Task<Result<VerifiedCustomArtwork>> VerifyAsync(Guid draftId, IReadOnlyCollection<Guid> ids,
        CrmActor actor, bool requireLinked, CancellationToken ct)
    {
        if (actor.UserId <= 0 || !await db.CustomDrafts.Where(CustomAccess.For(actor)).AnyAsync(x => x.PublicId == draftId, ct))
            return Result.NotFound();
        if (ids.Count > 30 || ids.Any(x => x == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            return Result.Invalid(new ValidationError("file", "ArtworkFileNotVerified"));
        if (ids.Count == 0) return Result.Success(new VerifiedCustomArtwork(draftId, Array.Empty<Guid>()));
        var spec = new PrivateFileVerificationSpec(draftId, ids, db.Set<CustomDraftAttachment>(), db.Set<CustomVersionAttachment>());
        var files = await SpecificationEvaluator.Default.GetQuery(db.Set<CustomPrivateFile>(), spec).ToArrayAsync(ct);
        if (files.Length != ids.Count || files.Any(x => x.State != CustomFileState.Uploaded
            || (requireLinked && !x.Linked) || (x.ExpiresAt <= time.GetUtcNow() && !x.Retained)))
            return Result.Invalid(new ValidationError("file", "ArtworkFileNotVerified"));
        foreach (var file in files)
        {
            var actual = await bytes.ReadAsync(draftId, file.FileId, ct);
            if (actual.Status == ResultStatus.Error) return Result.Error("private_file_storage_unavailable");
            if (!actual.IsSuccess || actual.Value.File.ContentType != file.ContentType
                || actual.Value.File.Length != file.Length || actual.Value.File.Sha256 != file.Sha256)
                return Result.Invalid(new ValidationError("file", "ArtworkFileNotVerified"));
        }
        return Result.Success(new VerifiedCustomArtwork(draftId, Array.AsReadOnly(files.Select(x => x.FileId).ToArray())));
    }
}
