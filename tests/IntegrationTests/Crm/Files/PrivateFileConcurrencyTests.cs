using Ardalis.Result;
using Crm.Application.Files.Finalize;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Infrastructure.Files.Cleanup;
using Crm.Infrastructure.Files.Finalize;
using Crm.Infrastructure.Files.Reading;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using IntegrationTests.Crm.Files.Support;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Crm.Files;

[NonParallelizable]
public sealed class PrivateFileConcurrencyTests : FileLifecycleFixture
{
    [Test]
    public async Task Independent_uploads_cannot_exceed_thirty_and_concurrent_finalization_has_one_revision_winner()
    {
        await using var db = Context();
        var draft = await Create(db);
        for (var index = 0; index < 29; index++) Assert.That((await Upload(db, draft.PublicId)).IsSuccess, Is.True);
        var uploads = await Task.WhenAll(Enumerable.Range(0, 4).Select(async _ =>
        {
            await using var connection = Context();
            return await Upload(connection, draft.PublicId);
        }));
        Assert.That(uploads.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(uploads.Count(x => x.Status == ResultStatus.Invalid), Is.EqualTo(3));
        Assert.That(await db.Set<CustomPrivateFile>().CountAsync(x => x.State == CustomFileState.Uploaded), Is.EqualTo(30));
        var fileId = uploads.Single(x => x.IsSuccess).Value.PublicId;
        var selections = await Task.WhenAll(Enumerable.Range(0, 2).Select(async index =>
        {
            await using var connection = Context();
            return await SetFiles(connection, draft, [new(fileId, index == 0, index == 1)]);
        }));
        Assert.That(selections.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(selections.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        Assert.That(await db.Set<CustomDraftAttachment>().CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Cleanup_winning_finalize_race_cannot_leave_linked_deleting_files_or_dangling_bytes()
    {
        await using var db = Context();
        var draft = await Create(db);
        var file = (await Upload(db, draft.PublicId)).Value;
        var gated = new GatedPrivateFileRead(Bytes, file.PublicId);
        var pending = new SetCustomFilesCommandHandler(new CustomMutationStore(db), new FinalizedCustomFilesWriter(db, gated, Clock))
            .Handle(new(draft.PublicId, Guid.NewGuid(), draft.Revision, [new(file.PublicId, true, false)], Manager), default).AsTask();
        try
        {
            await gated.Entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Clock.Now = Clock.Now.AddHours(25);
            await using var cleanupDb = Context();
            Assert.That(await new CustomTemporaryFileCleanup(cleanupDb, Bytes, Clock).CleanupAsync(10, default)
                .WaitAsync(TimeSpan.FromSeconds(15)), Is.EqualTo(1));
        }
        finally { gated.Release.TrySetResult(); }
        Assert.That((await pending.WaitAsync(TimeSpan.FromSeconds(15))).Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(await db.Set<CustomDraftAttachment>().CountAsync(), Is.Zero);
        Assert.That((await Bytes.ReadAsync(draft.PublicId, file.PublicId, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        var retainedDraft = await Create(db);
        var retained = (await Upload(db, retainedDraft.PublicId)).Value;
        Assert.That((await SetFiles(db, retainedDraft, [new(retained.PublicId, true, false)])).IsSuccess, Is.True);
        Clock.Now = Clock.Now.AddHours(25);
        Assert.That(await new CustomTemporaryFileCleanup(db, Bytes, Clock).CleanupAsync(10, default), Is.Zero);
        Assert.That(await db.Set<CustomPrivateFile>().AnyAsync(x => x.PublicId == retained.PublicId && x.State == CustomFileState.Uploaded), Is.True);
        Assert.That((await new CustomFileReader(db, Bytes).ContentAsync(retained.PublicId, null, null, Manager, default)).IsSuccess, Is.True);
    }

    [Test]
    public async Task Overlapping_cleanup_workers_are_idempotent_and_keep_expired_reservation_tombstone()
    {
        await using var db = Context();
        var draft = await Create(db);
        var file = (await Upload(db, draft.PublicId)).Value;
        Clock.Now = Clock.Now.AddHours(25);
        var counts = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            await using var connection = Context();
            return await new CustomTemporaryFileCleanup(connection, Bytes, Clock).CleanupAsync(1, default);
        }));
        Assert.That(counts.Sum(), Is.EqualTo(1));
        Assert.That(await db.Set<CustomPrivateFile>().AnyAsync(x => x.PublicId == file.PublicId && x.State == CustomFileState.Expired), Is.True);
        Assert.That(await new CustomTemporaryFileCleanup(db, Bytes, Clock).CleanupAsync(10, default), Is.Zero);
    }
}
