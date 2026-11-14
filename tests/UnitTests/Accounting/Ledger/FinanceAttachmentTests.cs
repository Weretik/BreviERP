using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Ardalis.Result;
using Microsoft.Extensions.Logging.Abstractions;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Attachments.Commands.DeleteFinanceAttachment;
using Accounting.Application.Features.Attachments.Commands.UploadFinanceAttachment;
using Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;
using Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;
using Accounting.Application.Features.Transactions.Shared;

namespace UnitTests.Accounting.Ledger;

/// <summary>«Файли» of an operation in the private storage (ACC-001 stage 4).</summary>
public sealed class FinanceAttachmentTests
{
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 1, 2, 3];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13];

    [Test]
    public async Task A_file_is_stored_privately_listed_and_logged()
    {
        using var harness = new AccountingTestHarness();
        var storage = new FakeAccountingFileStorage();
        var operation = await Operation(harness);

        var uploaded = await Upload(harness, storage, operation.Id, Pdf, @"C:\Users\me\Рахунок 12.pdf");
        // The list is in upload order; with the fixed test clock two uploads would share a moment and their
        // time-ordered ids, made within one millisecond, could come in either order.
        harness.Time.Advance(TimeSpan.FromSeconds(1));
        var png = await Upload(harness, storage, operation.Id, Png, "чек.png");
        var list = await new GetFinanceAttachmentsHandler(harness.Fresh())
            .Handle(new GetFinanceAttachmentsQuery(operation.Id), default);
        var content = await new GetFinanceAttachmentContentHandler(harness.Fresh(), storage,
                NullLogger<GetFinanceAttachmentContentHandler>.Instance)
            .Handle(new GetFinanceAttachmentContentQuery(uploaded.Value.Id), default);
        var history = await new GetFinanceHistoryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Transaction, operation.Id), default);
        var row = harness.Fresh().Attachments.Single(x => x.Id == FinanceAttachmentId.From(uploaded.Value.Id));
        using var read = new MemoryStream();
        await content.Value.Content.CopyToAsync(read);

        Assert.Multiple(() =>
        {
            Assert.That(uploaded.IsSuccess, Is.True);
            Assert.That(uploaded.Value.FileName, Is.EqualTo("Рахунок 12.pdf"), "the folder path is dropped");
            Assert.That(uploaded.Value.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(uploaded.Value.SizeBytes, Is.EqualTo(Pdf.Length));
            Assert.That(png.Value.ContentType, Is.EqualTo("image/png"));
            Assert.That(row.StorageKey, Is.EqualTo($"finance/{operation.Id:D}/{uploaded.Value.Id:D}.pdf"));
            Assert.That(row.Sha256, Has.Length.EqualTo(64));
            Assert.That(storage.Files.Keys, Has.Count.EqualTo(2));
            Assert.That(list.Value.Select(x => x.FileName), Is.EqualTo(new[] { "Рахунок 12.pdf", "чек.png" }));
            Assert.That(read.ToArray(), Is.EqualTo(Pdf));
            Assert.That(content.Value.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(history.Value.Count(x => x.Action == AuditAction.AttachmentAdd), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Only_a_pdf_jpeg_or_png_up_to_10_MiB_is_taken_by_its_content()
    {
        using var harness = new AccountingTestHarness();
        var storage = new FakeAccountingFileStorage();
        var operation = await Operation(harness);

        var renamedText = await Upload(harness, storage, operation.Id, "hello"u8.ToArray(), "рахунок.pdf");
        var empty = await Upload(harness, storage, operation.Id, [], "порожній.pdf");
        var large = await Upload(harness, storage, operation.Id,
            [.. Pdf, .. new byte[FinanceAttachment.MaxBytes]], "великий.pdf");
        var jpeg = await Upload(harness, storage, operation.Id, [0xFF, 0xD8, 0xFF, 0xE0, 0, 16], "photo.JPG");

        Assert.Multiple(() =>
        {
            Assert.That(renamedText.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(renamedText.ValidationErrors.Single().ErrorMessage, Is.EqualTo("Підходять лише PDF, JPG або PNG."));
            Assert.That(empty.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(large.ValidationErrors.Single().ErrorMessage, Is.EqualTo("Файл більший за 10 МБ."));
            Assert.That(jpeg.Value.ContentType, Is.EqualTo("image/jpeg"));
            Assert.That(storage.Files.Keys.Single(), Does.EndWith(".jpg"));
        });
    }

    [Test]
    public async Task No_files_for_a_voided_operation_or_without_a_working_storage()
    {
        using var harness = new AccountingTestHarness();
        var operation = await Operation(harness);
        var unconfigured = await Upload(harness, new FakeAccountingFileStorage(configured: false), operation.Id, Pdf, "a.pdf");
        var missing = await Upload(harness, new FakeAccountingFileStorage(), Guid.CreateVersion7(), Pdf, "a.pdf");
        var storage = new FakeAccountingFileStorage();
        var down = await Upload(harness, new FakeAccountingFileStorage { Down = true }, operation.Id, Pdf, "a.pdf");
        await new VoidFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(operation.Id, operation.Version, AccountingTestHarness.Director), default);
        var voided = await Upload(harness, storage, operation.Id, Pdf, "a.pdf");

        Assert.Multiple(() =>
        {
            Assert.That(unconfigured.Status, Is.EqualTo(ResultStatus.Unavailable));
            Assert.That(unconfigured.Errors.Single(), Does.StartWith("Сховище файлів бухгалтерії ще не налаштоване"));
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(down.Status, Is.EqualTo(ResultStatus.Unavailable));
            Assert.That(down.Errors.Single(), Is.EqualTo("Сховище файлів зараз недоступне. Спробуйте пізніше."));
            Assert.That(harness.Fresh().Attachments.Count(), Is.Zero, "no row without a stored file");
            Assert.That(voided.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(storage.Files, Is.Empty);
        });
    }

    [Test]
    public async Task A_deleted_file_leaves_the_list_and_the_storage_but_stays_in_the_history()
    {
        using var harness = new AccountingTestHarness();
        var storage = new FakeAccountingFileStorage();
        var operation = await Operation(harness);
        var uploaded = await Upload(harness, storage, operation.Id, Pdf, "акт.pdf");

        var deleted = await new DeleteFinanceAttachmentHandler(harness.Db, storage, harness.Time,
            NullLogger<DeleteFinanceAttachmentHandler>.Instance).Handle(
            new DeleteFinanceAttachmentCommand(uploaded.Value.Id, AccountingTestHarness.Director), default);
        var again = await new DeleteFinanceAttachmentHandler(harness.Db, storage, harness.Time,
            NullLogger<DeleteFinanceAttachmentHandler>.Instance).Handle(
            new DeleteFinanceAttachmentCommand(uploaded.Value.Id, AccountingTestHarness.Director), default);
        var list = await new GetFinanceAttachmentsHandler(harness.Fresh())
            .Handle(new GetFinanceAttachmentsQuery(operation.Id), default);
        var content = await new GetFinanceAttachmentContentHandler(harness.Fresh(), storage,
                NullLogger<GetFinanceAttachmentContentHandler>.Instance)
            .Handle(new GetFinanceAttachmentContentQuery(uploaded.Value.Id), default);
        var history = await new GetFinanceHistoryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Transaction, operation.Id), default);

        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(again.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(list.Value, Is.Empty);
            Assert.That(content.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(storage.Files, Is.Empty);
            var entry = history.Value.Single(x => x.Action == AuditAction.AttachmentDelete);
            Assert.That(entry.BeforeJson, Does.Contain("акт.pdf"));
            Assert.That(entry.AfterJson, Is.Null);
        });
    }

    private static async Task<FinanceTransactionResponse> Operation(AccountingTestHarness harness)
    {
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        return await Spend(harness, Fields(bank.Id, materials.Id, 300_00));
    }

    private static async Task<Result<FinanceAttachmentResponse>> Upload(AccountingTestHarness harness,
        FakeAccountingFileStorage storage, Guid transactionId, byte[] bytes, string fileName)
    {
        using var content = new MemoryStream(bytes);
        return await new UploadFinanceAttachmentHandler(harness.Db, storage, harness.Time,
                NullLogger<UploadFinanceAttachmentHandler>.Instance)
            .Handle(new UploadFinanceAttachmentCommand(transactionId, content, fileName, AccountingTestHarness.Director),
                default);
    }
}
