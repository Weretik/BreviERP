using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Reading;
using Crm.Application.Files.GetContent.Contracts;
using Crm.Application.Files.GetContent.Specifications;
using Crm.Application.Files.GetFiles.Specifications;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Files.Access;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Reading;

public sealed class CustomFileReader(CrmDbContext db, IPrivateFileStore bytes) : ICustomFileReader
{
    public async Task<Result<PrivateFileContent>> ContentAsync(Guid fileId, Guid? versionId, Guid? quoteId, CrmActor actor, CancellationToken ct)
    {
        if (fileId == Guid.Empty || actor.UserId <= 0 || (quoteId is not null && versionId is null)) return Result.NotFound();
        var metadata = await DescriptorAsync(fileId, versionId, quoteId, actor, ct);
        if (metadata is null) return Result.NotFound();
        var actual = await bytes.ReadAsync(metadata.DraftId, metadata.FileId, ct);
        if (!actual.IsSuccess) return actual.Status == ResultStatus.NotFound ? Result.NotFound() : Result.Error("private_file_storage_unavailable");
        if (actual.Value.File.ContentType != metadata.ContentType || actual.Value.File.Length != metadata.Length || actual.Value.File.Sha256 != metadata.Sha256)
            return Result.Error("private_file_integrity_failed");
        if (await DescriptorAsync(fileId, versionId, quoteId, actor, ct) is null) return Result.NotFound();
        return actual;
    }

    public async Task<Result<IReadOnlyList<PrivateFileView>>> ListAsync(Guid draftId, Guid? versionId, Guid? quoteId,
        CrmActor actor, CancellationToken ct)
    {
        if (actor.UserId <= 0 || draftId == Guid.Empty) return Result.NotFound();
        PrivateFileView[] rows;
        if (versionId is { } version)
        {
            if (!await CustomFileVersionScope.Query(db, actor, quoteId).AnyAsync(x => x.PublicId == version && x.Draft.PublicId == draftId, ct))
                return Result.NotFound();
            rows = await SpecificationEvaluator.Default.GetQuery(db.Set<CustomVersionAttachment>(), new VersionFileListSpec(version)).ToArrayAsync(ct);
        }
        else
        {
            if (quoteId is not null) return Result.NotFound();
            var root = await db.CustomDrafts.Where(CustomAccess.For(actor)).Where(x => x.PublicId == draftId).Select(x => x.Id).SingleOrDefaultAsync(ct);
            if (root.Value <= 0) return Result.NotFound();
            rows = await SpecificationEvaluator.Default.GetQuery(db.Set<CustomDraftAttachment>(), new DraftFileListSpec(root, draftId)).ToArrayAsync(ct);
        }
        return rows.Length > 30 ? Result.Error("private_file_limit_exceeded") : Result.Success<IReadOnlyList<PrivateFileView>>(rows);
    }

    public async Task<Result<IReadOnlyList<PrivateFileView>>> ProjectionAsync(Guid versionId, Guid quoteId, bool production, CrmActor actor, CancellationToken ct)
    {
        var draftId = await CustomFileVersionScope.Query(db, actor, quoteId).Where(x => x.PublicId == versionId)
            .Select(x => (Guid?)x.Draft.PublicId).SingleOrDefaultAsync(ct);
        if (draftId is null) return Result.NotFound();
        var files = await ListAsync(draftId.Value, versionId, quoteId, actor, ct);
        if (!files.IsSuccess) return files;
        return Result.Success<IReadOnlyList<PrivateFileView>>(files.Value.Where(x => production ? x.Production : x.ShowInQuote).ToArray());
    }

    private Task<PrivateFileReadDescriptor?> DescriptorAsync(Guid fileId, Guid? versionId, Guid? quoteId, CrmActor actor, CancellationToken ct)
    {
        if (versionId is { } version)
            return SpecificationEvaluator.Default.GetQuery(db.Set<CustomVersionAttachment>(), new VersionFileContentSpec(fileId, version,
                CustomFileVersionScope.Query(db, actor, quoteId))).SingleOrDefaultAsync(ct);
        return SpecificationEvaluator.Default.GetQuery(db.Set<CustomPrivateFile>(), new PrivateFileContentSpec(fileId,
            file => db.CustomDrafts.Where(CustomAccess.For(actor)).Any(root => root.Id == file.DraftId)
                && db.Set<CustomDraftAttachment>().Any(link => link.FileId == file.Id))).SingleOrDefaultAsync(ct);
    }
}
