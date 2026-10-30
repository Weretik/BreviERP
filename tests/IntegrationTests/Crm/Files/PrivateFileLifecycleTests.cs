using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Calculate;
using Crm.Application.CustomSewing.SaveVersion;
using Crm.Application.Files.Contracts;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Files.Cleanup;
using Crm.Infrastructure.Files.Reading;
using Crm.Infrastructure.Files.Storage;
using Crm.Infrastructure.Files.Upload;
using Crm.Infrastructure.Files.Verification;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using IntegrationTests.Crm.CustomSewing;
using IntegrationTests.Crm.Files.Support;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using Crm.Application.CustomSewing.SaveDraft;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;

namespace IntegrationTests.Crm.Files;

[NonParallelizable]
public sealed class PrivateFileLifecycleTests : FileLifecycleFixture
{
    [Test]
    public async Task Retry_recovers_a_known_reserved_object_written_before_a_crash()
    {
        await using var db = Context();
        var draft = await Create(db);
        var rootId = await db.CustomDrafts.Where(x => x.PublicId == draft.PublicId).Select(x => x.Id).SingleAsync();
        var key = Guid.NewGuid();
        var sha = Convert.ToHexString(SHA256.HashData(FileLifecycleTestData.Pdf));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{draft.PublicId:N}:application/pdf:{FileLifecycleTestData.Pdf.Length}:{sha}")));
        var reserved = CustomPrivateFile.Reserve(rootId, new CrmUserId(17), key, hash, "application/pdf",
            FileLifecycleTestData.Pdf.Length, sha, Clock.Now, Clock.Now.AddHours(24));
        db.Set<CustomPrivateFile>().Add(reserved);
        await db.SaveChangesAsync();
        Assert.That((await Bytes.WriteReservedAsync(draft.PublicId, reserved.PublicId, new MemoryStream(FileLifecycleTestData.Pdf),
            "application/pdf", reserved.ExpiresAt, default)).IsSuccess, Is.True);
        db.ChangeTracker.Clear();
        var recovered = await Upload(db, draft.PublicId, key);
        Assert.That(recovered.IsSuccess, Is.True);
        Assert.That(recovered.Value.PublicId, Is.EqualTo(reserved.PublicId));
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<CustomPrivateFile>().AnyAsync(x => x.PublicId == reserved.PublicId && x.State == CustomFileState.Uploaded), Is.True);
    }

    [Test]
    public async Task Artwork_requires_current_finalized_link_and_actual_integrity_before_pricing_and_version_pin()
    {
        await using var db = Context();
        var draft = await Create(db);
        var file = (await Upload(db, draft.PublicId)).Value;
        var verifier = new CustomArtworkVerifier(db, Bytes, Clock);
        Assert.That((await verifier.VerifyAsync(draft.PublicId, [file.PublicId], Manager, default)).Status, Is.EqualTo(ResultStatus.Invalid));
        draft = (await SetFiles(db, draft, [new(file.PublicId, false, true)])).Value;
        var proof = await verifier.VerifyAsync(draft.PublicId, [file.PublicId], Manager, default);
        Assert.That(proof.Value.FileIds, Is.EqualTo(new[] { file.PublicId }));
        Assert.That((await verifier.VerifyAsync(draft.PublicId, [file.PublicId], new(18, false), default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var configuration = draft.Configuration with { Applications = [new CustomApplication("print", "Front", file.PublicId, 0, 10, true)] };
        draft = (await new SaveCustomDraftCommandHandler(new CustomMutationStore(db))
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), configuration, Manager), default)).Value;
        var saved = await Freeze(db, draft);
        Assert.That(saved.CurrentVersion!.Configuration.Applications.Single().ArtworkFileId, Is.EqualTo(file.PublicId));
        Assert.That(await db.Set<CustomVersionAttachment>().CountAsync(), Is.EqualTo(1));
        await File.WriteAllBytesAsync(Path.Combine(Workspace, "private", draft.PublicId.ToString("N"), file.PublicId.ToString("N"), "content"),
            [.. FileLifecycleTestData.Pdf, 10]);
        Assert.That((await verifier.VerifyAsync(draft.PublicId, [file.PublicId], Manager, default)).IsSuccess, Is.False);
    }

    [Test]
    public async Task Actual_upload_requires_finalize_and_replays_only_same_payload_under_current_scope()
    {
        await using var db = Context();
        var draft = await Create(db);
        var key = Guid.NewGuid();
        var uploaded = await Upload(db, draft.PublicId, key);
        Assert.That(uploaded.IsSuccess, Is.True, string.Join(",", uploaded.Errors));
        Assert.That(uploaded.Value.IsFinalized, Is.False);
        var fileId = uploaded.Value.PublicId;
        var reader = new CustomFileReader(db, Bytes);
        Assert.That((await reader.ContentAsync(fileId, null, null, Manager, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var replay = await Upload(db, draft.PublicId, key);
        Assert.That(replay.Value.PublicId, Is.EqualTo(fileId));
        var changed = await Upload(db, draft.PublicId, key, bytes: [.. FileLifecycleTestData.Pdf, 10]);
        Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict));
        var foreign = new CrmActor(18, false);
        Assert.That((await Upload(db, draft.PublicId, actor: foreign)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await SetFiles(db, draft, [new(fileId, true, false)], actor: foreign)).Status, Is.EqualTo(ResultStatus.NotFound));
        var linkKey = Guid.NewGuid();
        var linked = await SetFiles(db, draft, [new(fileId, true, false)], linkKey);
        Assert.That(linked.IsSuccess, Is.True, string.Join(",", linked.Errors));
        Assert.That(linked.Value.Revision, Is.EqualTo(2));
        Assert.That((await SetFiles(db, draft, [new(fileId, true, false)], linkKey)).Value.Revision, Is.EqualTo(2));
        Assert.That((await SetFiles(db, draft, [new(fileId, false, true)], linkKey)).Status, Is.EqualTo(ResultStatus.Conflict));
        var actual = await reader.ContentAsync(fileId, null, null, Manager, default);
        Assert.That(actual.Value.Bytes.ToArray(), Is.EqualTo(FileLifecycleTestData.Pdf));
        Assert.That((await reader.ContentAsync(fileId, null, null, foreign, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await reader.ContentAsync(fileId, null, null, new(18, true), default)).IsSuccess, Is.True);
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Spoofed_or_oversized_payload_never_reserves_quota_and_foreign_file_cannot_be_linked()
    {
        await using var db = Context();
        var first = await Create(db);
        var second = await Create(db);
        var spoof = await Upload(db, first.PublicId, type: "image/png");
        Assert.That(spoof.ValidationErrors.Single().ErrorMessage, Is.EqualTo("unsupported_file_type"));
        var oversized = new byte[20 * 1024 * 1024 + 1];
        FileLifecycleTestData.Pdf.CopyTo(oversized, 0);
        var large = await Upload(db, first.PublicId, bytes: oversized);
        Assert.That(large.ValidationErrors.Single().ErrorMessage, Is.EqualTo("file_too_large"));
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(), Is.Zero);
        var file = (await Upload(db, first.PublicId)).Value;
        Assert.That((await SetFiles(db, second, [new(file.PublicId, true, true)])).Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(await db.Set<CustomDraftAttachment>().CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Version_file_metadata_flags_remain_immutable_and_cleanup_removes_only_unreferenced_temporary_bytes()
    {
        await using var db = Context();
        var draft = await Create(db);
        var firstFile = (await Upload(db, draft.PublicId)).Value;
        var privateFile = (await Upload(db, draft.PublicId)).Value;
        var linked = (await SetFiles(db, draft, [new(firstFile.PublicId, true, false), new(privateFile.PublicId, false, true)])).Value;
        var first = await Freeze(db, linked);
        var changed = (await SetFiles(db, first, [new(privateFile.PublicId, false, true)])).Value;
        var second = await Freeze(db, changed);
        var reader = new CustomFileReader(db, Bytes);
        var v1 = (await reader.ListAsync(draft.PublicId, first.CurrentVersion!.PublicId, null, Manager, default)).Value;
        var v2 = (await reader.ListAsync(draft.PublicId, second.CurrentVersion!.PublicId, null, Manager, default)).Value;
        Assert.That(v1, Has.Count.EqualTo(2));
        Assert.That(v1.Single(x => x.PublicId == firstFile.PublicId).ShowInQuote, Is.True);
        Assert.That(v2.Select(x => x.PublicId), Is.EqualTo(new[] { privateFile.PublicId }));
        Assert.That((await reader.ContentAsync(firstFile.PublicId, null, null, Manager, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await reader.ContentAsync(firstFile.PublicId, first.CurrentVersion.PublicId, null, Manager, default)).IsSuccess, Is.True);
        var temporary = (await Upload(db, draft.PublicId)).Value;
        Clock.Now = Clock.Now.AddHours(25);
        Assert.That(await new CustomTemporaryFileCleanup(db, Bytes, Clock).CleanupAsync(10, default), Is.EqualTo(1));
        Assert.That((await Bytes.ReadAsync(draft.PublicId, temporary.PublicId, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await Bytes.ReadAsync(draft.PublicId, firstFile.PublicId, default)).IsSuccess, Is.True);
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(x => x.State == CustomFileState.Expired), Is.EqualTo(1));
        var rewritten = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"""UPDATE crm."CustomVersionAttachments" SET "ShowInQuote"=false WHERE "VersionId" IN (SELECT "Id" FROM crm."CustomSewingVersions" WHERE "PublicId"={first.CurrentVersion.PublicId})"""));
        Assert.That(rewritten!.SqlState, Is.EqualTo("55000"));
        Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlRawAsync("DELETE FROM crm.\"CustomVersionAttachments\""));
    }

    [Test]
    public async Task Missing_storage_does_not_break_existing_custom_flow_but_file_upload_fails_before_reservation()
    {
        await using var db = Context();
        var draft = await Create(db);
        var unavailable = new ConfiguredPrivateFileStore(new PrivateFileStorageOptions());
        var upload = await new CustomFileUploader(db, unavailable, unavailable, Options, Clock).UploadAsync(
            new(draft.PublicId, Guid.NewGuid(), new MemoryStream(FileLifecycleTestData.Pdf), "application/pdf", Manager), default);
        Assert.That(upload.Errors, Does.Contain("private_file_storage_unavailable"));
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(), Is.Zero);
        var verifier = new CustomArtworkVerifier(db, unavailable, Clock);
        var pricing = new CustomSewingTestPrices();
        var preview = await new CalculateCustomSewingCommandHandler(new CustomMutationStore(db), pricing, verifier)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), Manager), default);
        var version = await new SaveCustomVersionCommandHandler(new CustomMutationStore(db), pricing, null, verifier)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), preview.Value.Preview!.Token, Manager), default);
        Assert.That(version.IsSuccess, Is.True);
    }
}
