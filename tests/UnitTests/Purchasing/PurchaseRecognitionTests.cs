using System.Text.Json;
using Ardalis.Result;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;
using Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;
using Purchasing.Application.Features.Documents.Queries.GetPurchaseDocumentTemplate;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Application.Abstractions.Sources;
using Purchasing.Application.Features.Documents.Shared.Recognition;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

namespace UnitTests.Purchasing;

/// <summary>PUR-001 stage 3: «Розпізнати», «Розпізнати всі нові», «Завантажити JSON».</summary>
public sealed class PurchaseRecognitionTests
{
    private const string By = "buh@brevi.com.ua";
    private const string Excel = "https://www.dropbox.com/scl/fi/xyz/price.xlsx?rlkey=y&dl=0";
    private PurchasingTestHarness harness = null!;
    private string fileId = null!;

    [SetUp]
    public async Task SetUp()
    {
        harness = new PurchasingTestHarness();
        harness.Sheet.Rows =
        [
            SheetRow.Of("391813863", messageId: "1", dateTime: "05.11.2026 10:00:00"),
            SheetRow.Of("391813864", messageId: "2", dateTime: "04.11.2026 10:00:00", fileLinks: Excel),
            SheetRow.Of("391813865", messageId: "3", dateTime: "03.11.2026 10:00:00", fileLinks: "http://example.com/a.pdf"),
        ];
        await harness.Sync().RunAsync(CancellationToken.None);
        fileId = harness.Fresh().Files.Single(x => x.Url == SheetRow.Dropbox).Id;
    }

    [TearDown]
    public void TearDown() => harness.Dispose();

    private Task<Result<PurchaseRecognitionResponse>> RecognizeAsync(string? id = null) =>
        harness.Recognition().RecognizeAsync(id ?? fileId, By, CancellationToken.None);

    private async Task<PurchaseInvoicePage> ListAsync(PurchaseInvoiceFilter filter = PurchaseInvoiceFilter.All) =>
        (await new GetPurchaseInvoicesHandler(harness.Db, harness.Suppliers, harness.Payments)
            .Handle(new GetPurchaseInvoicesQuery(1, 50, null, filter), CancellationToken.None)).Value;

    private Task<Result<PurchaseDocumentSaveResponse>> SaveAsync(string json, bool replace = false)
    {
        using var parsed = JsonDocument.Parse(json);
        return new SaveCheckedPurchaseDocumentHandler(harness.Db, harness.Time)
            .Handle(new SaveCheckedPurchaseDocumentCommand(fileId, parsed.RootElement.Clone(), replace, By), CancellationToken.None)
            .AsTask();
    }

    private static string Manual(string json) =>
        json.TrimStart().Insert(1, $$"""
            "sourceUID":"391813863","originalFileLink":"{{SheetRow.Dropbox}}",
            """);

    [Test]
    public async Task A_valid_fast_answer_is_saved_and_shown_on_the_card()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));

        var result = await RecognizeAsync();

        Assert.That(result.Value.Result, Is.EqualTo(PurchaseRecognitionResult.Recognized));
        Assert.That(harness.Recognizer.Requests.Single().Strong, Is.False);
        Assert.That(harness.Recognizer.Requests.Single().SchemaJson, Is.EqualTo(PurchaseRecognitionPrompt.SchemaJson));
        var card = (await ListAsync()).Items.First();
        var document = card.Links.Single().Document!;
        Assert.That((document.DocumentNumber, document.Vat, document.TotalPayable, document.Items.Count),
            Is.EqualTo(("СФ-15", PurchaseDocumentVat.WithVat, "1560.00", 2)));
        Assert.That((document.Source, document.Model, document.CheckedBy), Is.EqualTo((PurchaseDocumentSource.Ai, "gpt-6-luna", By)));
        Assert.That(card.State, Is.EqualTo(PurchaseInvoiceState.Recognized));
        Assert.That(card.Issues, Is.Empty);
        Assert.That(harness.Fresh().CheckedDocuments.Single().SourceUid, Is.EqualTo("391813863"));
    }

    [Test]
    public async Task The_register_filters_by_invoice_date_and_vat_of_the_checked_facts()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));
        await RecognizeAsync();

        async Task<IEnumerable<string>> Uids(DateOnly? from = null, DateOnly? to = null, PurchaseDocumentVat? vat = null) =>
            (await new GetPurchaseInvoicesHandler(harness.Db, harness.Suppliers, harness.Payments)
                .Handle(new GetPurchaseInvoicesQuery(1, 50, null, PurchaseInvoiceFilter.All, null, from, to, vat),
                    CancellationToken.None)).Value.Items.Select(x => x.SourceUid);

        Assert.That(await Uids(new DateOnly(2026, 11, 3), new DateOnly(2026, 11, 3)), Is.EqualTo(new[] { "391813863" }));
        Assert.That(await Uids(from: new DateOnly(2026, 11, 4)), Is.Empty);
        Assert.That(await Uids(vat: PurchaseDocumentVat.WithVat), Is.EqualTo(new[] { "391813863" }));
        Assert.That(await Uids(vat: PurchaseDocumentVat.WithoutVat), Is.Empty);
        var saved = harness.Fresh().CheckedDocuments.Single();
        Assert.That((saved.DocumentDate, saved.Vat),
            Is.EqualTo(((DateOnly?)new DateOnly(2026, 11, 3), global::Purchasing.Domain.Documents.PurchaseVatMode.WithVat)));
    }

    [Test]
    public async Task A_failed_check_is_retried_once_with_the_strong_model()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop.Replace("1560.00", "1500.00")));
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop, "gpt-6.1-terra"));

        var result = await RecognizeAsync();

        Assert.That((result.Value.Result, result.Value.Model, result.Value.CostUsd),
            Is.EqualTo((PurchaseRecognitionResult.Recognized, "gpt-6.1-terra", 0.004m)));
        Assert.That(harness.Recognizer.Requests.Select(x => x.Strong), Is.EqualTo(new[] { false, true }));
    }

    [Test]
    public async Task Two_failed_answers_put_the_card_in_needs_review_with_the_reason()
    {
        var wrong = PurchaseDocumentRulesTests.VatOnTop.Replace("1560.00", "1500.00");
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(wrong));
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(wrong, "gpt-6.1-terra"));

        var result = await RecognizeAsync();

        Assert.That(result.Value.Result, Is.EqualTo(PurchaseRecognitionResult.NeedsReview));
        var card = (await ListAsync(PurchaseInvoiceFilter.NeedsReview)).Items.Single(x => x.Links.Any(l => l.Id == fileId));
        Assert.That(card.State, Is.EqualTo(PurchaseInvoiceState.NeedsReview));
        Assert.That(card.Issues, Is.EqualTo(new[] { "invoice.pdf: Сума позицій 1300.00 + ПДВ 260.00 = 1560.00, а до оплати 1500.00." }));
        Assert.That(card.Links.Single().Recognition!.Model, Is.EqualTo("gpt-6.1-terra"));
        Assert.That(harness.Fresh().CheckedDocuments, Is.Empty);
    }

    [Test]
    public async Task An_unavailable_ai_records_nothing()
    {
        var result = await RecognizeAsync();

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Unavailable));
        Assert.That(harness.Fresh().RecognitionAttempts, Is.Empty);
    }

    [Test]
    public async Task Excel_is_skipped_with_a_reason_and_a_dropbox_failure_is_recorded()
    {
        var excel = harness.Fresh().Files.Single(x => x.Url == Excel).Id;
        var skipped = await RecognizeAsync(excel);
        Assert.That(skipped.Value.Result, Is.EqualTo(PurchaseRecognitionResult.Skipped));

        harness.Downloader.Failure = new PurchaseFileUnavailableException("Dropbox не відповідає.");
        var failed = await RecognizeAsync();
        Assert.That((failed.Value.Result, failed.Value.Problems.Single()), Is.EqualTo((PurchaseRecognitionResult.Failed, "Dropbox не відповідає.")));
        Assert.That(harness.Recognizer.Requests, Is.Empty);
    }

    [Test]
    public async Task Recognising_again_replaces_the_facts_and_keeps_the_old_ones_in_history()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));
        await RecognizeAsync();
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop.Replace("СФ-15", "СФ-16")));

        await RecognizeAsync();

        using var db = harness.Fresh();
        Assert.That(db.CheckedDocuments.Single().PayloadJson, Does.Contain("СФ-16"));
        Assert.That(db.CheckedDocumentHistory.Single().PayloadJson, Does.Contain("СФ-15"));
        Assert.That(db.RecognitionAttempts.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Recognising_while_the_sync_runs_is_refused()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));
        harness.Db.LockHeld = true;

        var result = await RecognizeAsync();

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Recognize_new_takes_untried_safe_files_newest_first_and_stops_when_the_ai_is_off()
    {
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));
        var handler = new RecognizeNewPurchaseFilesHandler(harness.Db, harness.Recognition());

        var first = (await handler.Handle(new RecognizeNewPurchaseFilesCommand(By, 1), CancellationToken.None)).Value;
        Assert.That((first.Recognized, first.Remaining, first.StopReason), Is.EqualTo((1, 1, (string?)null)));

        var second = (await handler.Handle(new RecognizeNewPurchaseFilesCommand(By), CancellationToken.None)).Value;
        Assert.That((second.Skipped, second.Remaining), Is.EqualTo((1, 0)), "the Excel file is tried once and skipped");

        var excel = harness.Fresh().Files.Single(x => x.Url == Excel).Id;
        Assert.That(harness.Fresh().RecognitionAttempts.Select(x => x.FileId), Is.EquivalentTo(new[] { fileId, excel }));

        harness.Sheet.Rows.Add(SheetRow.Of("391813866", messageId: "4", fileLinks: "https://www.dropbox.com/scl/fi/n/new.pdf?dl=0"));
        await harness.Sync().RunAsync(CancellationToken.None);
        var stopped = (await handler.Handle(new RecognizeNewPurchaseFilesCommand(By), CancellationToken.None)).Value;
        Assert.That((stopped.StopReason, stopped.Remaining), Is.EqualTo(("ШІ вимкнено в «Налаштування ШІ».", 1)));
    }

    [Test]
    public async Task A_manual_file_is_saved_once_and_different_facts_need_replace()
    {
        var json = Manual(PurchaseDocumentRulesTests.WithoutVat);

        var saved = await SaveAsync(json);
        var again = await SaveAsync(json);
        var other = await SaveAsync(json.Replace("110776", "110777"));
        var replaced = await SaveAsync(json.Replace("110776", "110777"), replace: true);

        Assert.That((saved.Value.Unchanged, again.Value.Unchanged), Is.EqualTo((false, true)));
        Assert.That(other.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(replaced.Value.Replaced, Is.True);
        using var db = harness.Fresh();
        Assert.That(db.CheckedDocuments.Single().Origin, Is.EqualTo(global::Purchasing.Domain.Documents.PurchaseDocumentOrigin.Manual));
        Assert.That(db.CheckedDocumentHistory.Single().PayloadJson, Does.Contain("110776"));
    }

    [Test]
    public async Task A_manual_file_with_a_foreign_uid_or_bad_sums_is_refused()
    {
        var json = Manual(PurchaseDocumentRulesTests.WithoutVat);
        var foreign = await SaveAsync(json.Replace("391813863", "391813864"));
        var link = await SaveAsync(json.Replace(SheetRow.Dropbox, SheetRow.Dropbox + "&x=1"));
        var sums = await SaveAsync(json.Replace("\"totalPayable\":\"8000.00\"", "\"totalPayable\":\"7000.00\""));

        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(link.Status, Is.EqualTo(ResultStatus.Invalid), string.Join(";", link.Errors));
        Assert.That(link.ValidationErrors.Select(x => x.ErrorMessage), Has.Some.Contains("originalFileLink"));
        Assert.That(sums.ValidationErrors.Select(x => x.ErrorMessage), Has.Some.Contains("а до оплати 7000.00"));
    }

    [Test]
    public async Task The_template_has_the_identity_and_the_last_ai_answer()
    {
        var wrong = PurchaseDocumentRulesTests.VatOnTop.Replace("1560.00", "1500.00");
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(wrong));
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(wrong));
        await RecognizeAsync();

        var template = (await new GetPurchaseDocumentTemplateHandler(harness.Db)
            .Handle(new GetPurchaseDocumentTemplateQuery(fileId), CancellationToken.None)).Value;

        using var json = JsonDocument.Parse(template);
        Assert.That(json.RootElement.GetProperty("sourceUID").GetString(), Is.EqualTo("391813863"));
        Assert.That(json.RootElement.GetProperty("originalFileLink").GetString(), Is.EqualTo(SheetRow.Dropbox));
        Assert.That(json.RootElement.GetProperty("documentNumber").GetString(), Is.EqualTo("СФ-15"));
        Assert.That(template, Does.Contain("ТОВ Текстиль-Контакт"));
    }
}
