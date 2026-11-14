using System.Text.Json;
using Ardalis.Result;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;
using Purchasing.Application.Features.Register.Commands.LinkPurchasePayment;
using Purchasing.Application.Features.Register.Commands.UnlinkPurchasePayment;
using Purchasing.Application.Features.Register.Commands.ReviewPurchaseInvoice;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;
using Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

namespace UnitTests.Purchasing.Invoices;

/// <summary>Accounting stage 9: an invoice is paid by expenses of «Бухгалтерія» linked by hand.</summary>
public sealed class PurchasePaymentTests
{
    private const string By = "director@brevi.com.ua";
    private static readonly Guid Paid = Guid.Parse("0193a8a0-0000-7000-8000-000000000001");
    private static readonly Guid Other = Guid.Parse("0193a8a0-0000-7000-8000-000000000002");
    private static readonly Guid Income = Guid.Parse("0193a8a0-0000-7000-8000-000000000003");
    private PurchasingTestHarness harness = null!;
    private string messageId = null!;

    [SetUp]
    public async Task SetUp()
    {
        harness = new PurchasingTestHarness();
        harness.Sheet.Rows = [SheetRow.Of("100", messageId: "1")];
        await harness.Sync().RunAsync(CancellationToken.None);
        messageId = harness.Fresh().Messages.Single().Id;
        harness.Payments.Operations.AddRange(
        [
            Operation(Paid, 156_000, new DateOnly(2026, 11, 6), "Оплата рахунку 15"),
            Operation(Other, 50_000, new DateOnly(2026, 11, 7), "Нитки"),
            Operation(Income, 156_000, new DateOnly(2026, 11, 7), "Повернення") with { Expense = false },
        ]);
        harness.Payments.SupplierOf[Paid] = 7;
        harness.Payments.SupplierOf[Other] = 7;
    }

    [TearDown]
    public void TearDown() => harness.Dispose();

    private static PurchasingPayment Operation(Guid id, long amount, DateOnly date, string description) =>
        new(id, date, amount, "UAH", "Банк", description, "Текстиль-Контакт", true, true);

    private Task<Result<PurchaseInvoicePaymentsResponse>> LinkAsync(Guid id, int version) =>
        new LinkPurchasePaymentHandler(harness.Db, harness.Payments, harness.Time).Handle(new LinkPurchasePaymentCommand(messageId, id, version, By), CancellationToken.None).AsTask();

    private Task<Result<PurchaseInvoicePaymentsResponse>> UnlinkAsync(Guid id, int version) =>
        new UnlinkPurchasePaymentHandler(harness.Db, harness.Payments, harness.Time).Handle(new UnlinkPurchasePaymentCommand(messageId, id, version, By), CancellationToken.None).AsTask();

    private async Task<PurchaseInvoiceResponse> CardAsync() =>
        (await new GetPurchaseInvoicesHandler(harness.Db, harness.Suppliers, harness.Payments)
            .Handle(new GetPurchaseInvoicesQuery(1, 50, null, PurchaseInvoiceFilter.All), CancellationToken.None))
        .Value.Items.Single();

    private async Task<IReadOnlyList<PurchasePaymentCandidateResponse>> CandidatesAsync(string? search = null) =>
        (await new GetPurchasePaymentCandidatesHandler(harness.Db, harness.Payments)
            .Handle(new GetPurchasePaymentCandidatesQuery(messageId, search), CancellationToken.None)).Value;

    private async Task SaveFactsAsync()
    {
        var fileId = harness.Fresh().Files.Single().Id;
        var json = PurchaseDocumentRulesTests.VatOnTop.TrimStart().Insert(1, $$"""
            "sourceUID":"100","originalFileLink":"{{SheetRow.Dropbox}}",
            """);
        using var parsed = JsonDocument.Parse(json);
        var saved = await new SaveCheckedPurchaseDocumentHandler(harness.Db, harness.Time)
            .Handle(new SaveCheckedPurchaseDocumentCommand(fileId, parsed.RootElement.Clone(), false, By),
                CancellationToken.None);
        Assert.That(saved.IsSuccess, Is.True, string.Join("; ", saved.ValidationErrors.Select(x => x.ErrorMessage)));
    }

    [Test]
    public async Task Links_an_expense_bumps_the_version_and_shows_it_on_the_card_without_problems()
    {
        await SaveFactsAsync();
        var version = (await CardAsync()).ReviewVersion;

        var result = await LinkAsync(Paid, version);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ReviewVersion, Is.EqualTo(version + 1));
        Assert.That(result.Value.TotalPayableMinor, Is.EqualTo(156_000));
        Assert.That(result.Value.PaymentIssues, Is.Empty);
        var card = await CardAsync();
        Assert.That(card.Payments.Single().TransactionId, Is.EqualTo(Paid));
        Assert.That(card.Payments.Single().LinkedBy, Is.EqualTo(By));
        Assert.That(card.PaymentIssues, Is.Empty);
    }

    [Test]
    public async Task Marks_a_different_amount_and_a_voided_payment_as_problems()
    {
        await SaveFactsAsync();
        var linked = await LinkAsync(Other, (await CardAsync()).ReviewVersion);

        Assert.That(linked.Value.PaymentIssues, Is.EqualTo(new[] { "Оплачено 500,00 ₴, а сума рахунку 1 560,00 ₴." }));

        harness.Payments.Operations[1] = harness.Payments.Operations[1] with { Active = false };
        var card = await CardAsync();
        Assert.That(card.Payments.Single().Voided, Is.True);
        Assert.That(card.PaymentIssues, Is.EqualTo(new[] { "Оплату 07.11.2026 на 500,00 ₴ анульовано в «Бухгалтерії»." }));
        Assert.That(card.State, Is.Not.EqualTo(PurchaseInvoiceState.NeedsReview));
    }

    [Test]
    public async Task Refuses_an_income_a_stale_version_and_a_second_link_of_the_same_operation()
    {
        var income = await LinkAsync(Income, 0);
        Assert.That(income.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(income.ValidationErrors.Single().ErrorMessage, Is.EqualTo(LinkPurchasePaymentHandler.NotExpense));

        Assert.That((await LinkAsync(Paid, 0)).IsSuccess, Is.True);
        var stale = await LinkAsync(Other, 0);
        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(stale.Errors.Single(), Is.EqualTo(ReviewPurchaseInvoiceHandler.Stale));
        var twice = await LinkAsync(Paid, 1);
        Assert.That(twice.Errors.Single(), Is.EqualTo(LinkPurchasePaymentHandler.AlreadyLinked));
    }

    [Test]
    public async Task Unlinking_keeps_the_link_in_history_and_allows_linking_again()
    {
        await LinkAsync(Paid, 0);

        var removed = await UnlinkAsync(Paid, 1);

        Assert.That(removed.IsSuccess, Is.True);
        Assert.That(removed.Value.Payments, Is.Empty);
        Assert.That((await UnlinkAsync(Paid, 2)).Errors.Single(), Is.EqualTo(UnlinkPurchasePaymentHandler.NotLinked));
        using (var db = harness.Fresh())
        {
            var history = db.Payments.Single();
            Assert.That((history.UnlinkedBy, history.UnlinkedAt), Is.EqualTo((By, harness.Time.GetUtcNow())));
        }

        Assert.That((await LinkAsync(Paid, 2)).IsSuccess, Is.True);
        using var fresh = harness.Fresh();
        Assert.That(fresh.Payments.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Offers_the_supplier_expenses_with_the_invoice_amount_first_and_hides_linked_ones()
    {
        await SaveFactsAsync();
        await harness.Review().Handle(new ReviewPurchaseInvoiceCommand(messageId, 7, null, 0, By), CancellationToken.None);

        var offered = await CandidatesAsync();

        Assert.That(offered.Select(x => (x.TransactionId, x.SameAmount)), Is.EqualTo(new[] { (Paid, true), (Other, false) }));
        Assert.That(harness.Payments.CandidateRequests.Last(), Is.EqualTo(((int?)7, (long?)156_000, (string?)null)));

        await LinkAsync(Paid, 1);
        Assert.That((await CandidatesAsync()).Select(x => x.TransactionId), Is.EqualTo(new[] { Other }));
        Assert.That((await CandidatesAsync("  нитки ")).Single().TransactionId, Is.EqualTo(Other));
        Assert.That(harness.Payments.CandidateRequests.Last().Search, Is.EqualTo("нитки"));
    }
}
