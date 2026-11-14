using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sync;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace UnitTests.Purchasing;

public sealed class PurchaseRegisterSyncTests
{
    private PurchasingTestHarness harness = null!;

    [SetUp]
    public void SetUp() => harness = new PurchasingTestHarness();

    [TearDown]
    public void TearDown() => harness.Dispose();

    [Test]
    public async Task FirstSyncAddsMessagesRowsRevisionsAndFiles()
    {
        harness.Sheet.Rows = [SheetRow.Of("1"), SheetRow.Of("2", messageId: "11", fileLinks: "")];

        var result = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True, string.Join(" ", result.Errors));
        Assert.That(result.Value.RecordCount, Is.EqualTo(2));
        Assert.That(result.Value.FileCount, Is.EqualTo(1));
        await using var db = harness.Fresh();
        Assert.That(await db.Messages.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.SourceRevisions.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.Files.CountAsync(), Is.EqualTo(1));
        var state = await db.SyncStates.SingleAsync();
        Assert.That(state.LastSuccessAt, Is.EqualTo(harness.Time.GetUtcNow()));
        Assert.That(state.Error, Is.Null);
    }

    [Test]
    public async Task RepeatingTheSameSheetChangesNothing()
    {
        harness.Sheet.Rows = [SheetRow.Of("1")];
        await harness.Sync().RunAsync(CancellationToken.None);

        var again = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(again.Value, Has.Property("MessagesAdded").EqualTo(0).And.Property("MessagesUpdated").EqualTo(0)
            .And.Property("RowsAdded").EqualTo(0).And.Property("RowsUpdated").EqualTo(0).And.Property("FilesAdded").EqualTo(0));
        await using var db = harness.Fresh();
        Assert.That(await db.SourceRevisions.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task MovedRowsKeepTheirIdsAndOnlyChangeTheRowNumber()
    {
        harness.Sheet.Rows = [SheetRow.Of("1"), SheetRow.Of("2", messageId: "11")];
        await harness.Sync().RunAsync(CancellationToken.None);
        harness.Sheet.Rows = [SheetRow.Of("2", messageId: "11"), new string?[14], SheetRow.Of("1")];

        await harness.Sync().RunAsync(CancellationToken.None);

        await using var db = harness.Fresh();
        var rows = await db.SourceRows.OrderBy(x => x.SourceUid).ToListAsync();
        Assert.That(rows.Select(x => x.RowNumber), Is.EqualTo(new[] { 4, 2 }));
        Assert.That(await db.Messages.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.SourceRevisions.CountAsync(), Is.EqualTo(2), "moving a row is not a new revision");
    }

    [Test]
    public async Task RowsAndLinksMissingFromTheSheetBecomeInactiveAndStay()
    {
        var other = "https://www.dropbox.com/scl/fi/other.pdf";
        harness.Sheet.Rows = [SheetRow.Of("1", fileLinks: $"{SheetRow.Dropbox} {other}"), SheetRow.Of("2", messageId: "11")];
        await harness.Sync().RunAsync(CancellationToken.None);
        harness.Sheet.Rows = [SheetRow.Of("1")];

        var result = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(result.Value.RowsDeactivated, Is.EqualTo(1));
        Assert.That(result.Value.FilesDeactivated, Is.EqualTo(2));
        await using var db = harness.Fresh();
        Assert.That(await db.SourceRows.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.SourceRows.CountAsync(x => x.Active), Is.EqualTo(1));
        Assert.That(await db.Files.Where(x => x.Active).Select(x => x.Url).ToListAsync(), Is.EqualTo(new[] { SheetRow.Dropbox }));
    }

    [Test]
    public async Task ManualSupplierLinkAndNoteSurviveTheSync()
    {
        harness.Sheet.Rows = [SheetRow.Of("1", text: "Старий текст")];
        await harness.Sync().RunAsync(CancellationToken.None);
        await using (var db = harness.Fresh())
        {
            var message = await db.Messages.SingleAsync();
            db.Entry(message).Property(x => x.SupplierId).CurrentValue = 7;
            db.Entry(message).Property(x => x.ReviewNote).CurrentValue = "Перевірити кількість";
            db.Entry(message).Property(x => x.ReviewVersion).CurrentValue = 3;
            await db.SaveChangesAsync();
        }

        harness.Sheet.Rows = [SheetRow.Of("1", text: "Новий текст", status: "СПЛАЧЕНО")];
        await harness.Sync().RunAsync(CancellationToken.None);

        await using var check = harness.Fresh();
        var updated = await check.Messages.SingleAsync();
        Assert.That(updated.Text, Is.EqualTo("Новий текст"));
        Assert.That(updated.PaymentState, Is.EqualTo(PaymentState.MarkedPaid));
        Assert.That((updated.SupplierId, updated.ReviewNote, updated.ReviewVersion), Is.EqualTo((7, "Перевірити кількість", 3)));
        Assert.That(await check.SourceRevisions.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task AUidThatChangesItsTelegramMessageStopsTheSyncWithoutChanges()
    {
        harness.Sheet.Rows = [SheetRow.Of("1", messageId: "10")];
        await harness.Sync().RunAsync(CancellationToken.None);
        harness.Sheet.Rows = [SheetRow.Of("1", messageId: "99"), SheetRow.Of("2", messageId: "11")];

        var result = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
        Assert.That(result.Errors.Single(), Does.Contain("UID 1"));
        await using var db = harness.Fresh();
        Assert.That(await db.Messages.CountAsync(), Is.EqualTo(1));
        Assert.That((await db.SyncStates.SingleAsync()).Error, Does.Contain("UID 1"));
    }

    [Test]
    public async Task UnreadableGoogleLeavesTheRegisterAndRecordsTheReason()
    {
        harness.Sheet.Rows = [SheetRow.Of("1")];
        await harness.Sync().RunAsync(CancellationToken.None);
        harness.Sheet.Failure = new PurchaseSheetUnavailableException("Google не дає доступу");

        var result = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Unavailable));
        await using var db = harness.Fresh();
        Assert.That(await db.SourceRows.CountAsync(x => x.Active), Is.EqualTo(1));
        var state = await db.SyncStates.SingleAsync(x => x.Id == PurchaseSyncState.GoogleFinance);
        Assert.That(state.Error, Is.EqualTo("Google не дає доступу"));
        Assert.That(state.RecordCount, Is.EqualTo(1), "the last successful counts stay");
    }

    [Test]
    public async Task ASecondSyncWhileOneRunsIsRefused()
    {
        harness.Sheet.Rows = [SheetRow.Of("1")];
        harness.Db.LockHeld = true;

        var result = await harness.Sync().RunAsync(CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(result.Errors.Single(), Is.EqualTo(PurchaseRegisterSync.AlreadyRunning));
        await using var db = harness.Fresh();
        Assert.That(await db.Messages.CountAsync(), Is.Zero);
    }
}
