using Ardalis.Result;
using Purchasing.Application.Contracts;
using Purchasing.Application.Register.GetInvoices;
using Purchasing.Application.Register.Review;

namespace UnitTests.Purchasing;

/// <summary>PUR-001 stage 2: manual supplier link and note with an optimistic version and history.</summary>
public sealed class PurchaseInvoiceReviewTests
{
    private PurchasingTestHarness harness = null!;
    private string messageId = null!;

    [SetUp]
    public async Task SetUp()
    {
        harness = new PurchasingTestHarness();
        harness.Sheet.Rows = [SheetRow.Of("100", messageId: "1"), SheetRow.Of("200", messageId: "2")];
        await harness.Sync().RunAsync(CancellationToken.None);
        messageId = harness.Fresh().Messages.OrderBy(x => x.TelegramKey).First().Id;
    }

    [TearDown]
    public void TearDown() => harness.Dispose();

    private Task<Result<PurchaseInvoiceReviewResponse>> ReviewAsync(int? supplierId, string? note, int version,
        string? id = null) =>
        harness.Review().Handle(new ReviewPurchaseInvoiceCommand(id ?? messageId, supplierId, note, version,
            "buh@brevi.com.ua"), CancellationToken.None).AsTask();

    private async Task<PurchaseInvoicePage> ListAsync(PurchaseInvoiceFilter filter = PurchaseInvoiceFilter.All,
        int? supplierId = null) =>
        (await new GetPurchaseInvoicesQueryHandler(harness.Db, harness.Suppliers)
            .Handle(new GetPurchaseInvoicesQuery(1, 50, null, filter, supplierId), CancellationToken.None)).Value;

    [Test]
    public async Task The_supplier_card_lists_only_the_invoices_linked_to_that_supplier()
    {
        await ReviewAsync(7, null, 0);

        var linked = await ListAsync(supplierId: 7);
        Assert.That(linked.Total, Is.EqualTo(1));
        Assert.That(linked.Items.Single().MessageId, Is.EqualTo(messageId));
        Assert.That((await ListAsync(supplierId: 8)).Total, Is.Zero);
    }

    [Test]
    public async Task Saves_supplier_and_trimmed_note_bumps_the_version_and_writes_history()
    {
        var result = await ReviewAsync(7, "  Тканина на березень  ", 0);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SupplierName, Is.EqualTo("Текстиль-Контакт"));
        Assert.That((result.Value.ReviewNote, result.Value.ReviewVersion), Is.EqualTo(("Тканина на березень", 1)));
        Assert.That(result.Value.ReviewedBy, Is.EqualTo("buh@brevi.com.ua"));
        Assert.That(result.Value.ReviewedAt, Is.EqualTo(harness.Time.GetUtcNow()));

        using var db = harness.Fresh();
        var change = db.ReviewChanges.Single();
        Assert.That((change.MessageId, change.Reviewer), Is.EqualTo((messageId, "buh@brevi.com.ua")));
        Assert.That(change.Before, Is.EqualTo(new global::Purchasing.Domain.Messages.PurchaseReview(null, null, 0)));
        Assert.That(change.After, Is.EqualTo(new global::Purchasing.Domain.Messages.PurchaseReview(7, "Тканина на березень", 1)));
    }

    [Test]
    public async Task A_stale_version_is_refused_and_keeps_the_newer_change()
    {
        await ReviewAsync(7, "перша", 0);

        var stale = await ReviewAsync(8, "друга", 0);

        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(stale.Errors, Is.EqualTo(new[] { ReviewPurchaseInvoiceCommandHandler.Stale }));
        using var db = harness.Fresh();
        Assert.That(db.Messages.Single(x => x.Id == messageId).ReviewNote, Is.EqualTo("перша"));
        Assert.That(db.ReviewChanges.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Unlinking_and_clearing_the_note_is_a_change_but_saving_the_same_values_is_not()
    {
        await ReviewAsync(7, "нотатка", 0);

        var same = await ReviewAsync(7, "нотатка ", 1);
        Assert.That(same.Value.ReviewVersion, Is.EqualTo(1));

        var cleared = await ReviewAsync(null, "   ", 1);
        Assert.That((cleared.Value.SupplierId, cleared.Value.ReviewNote, cleared.Value.ReviewVersion),
            Is.EqualTo(((int?)null, (string?)null, 2)));
        using var db = harness.Fresh();
        Assert.That(db.ReviewChanges.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Unknown_or_inactive_supplier_and_unknown_card_are_not_found()
    {
        Assert.That((await ReviewAsync(404, null, 0)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await ReviewAsync(9, null, 0)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await ReviewAsync(7, null, 0, id: new string('0', 64))).Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task A_supplier_that_became_inactive_may_stay_linked()
    {
        await ReviewAsync(8, null, 0);
        harness.Suppliers.Suppliers[1] = harness.Suppliers.Suppliers[1] with { Active = false };

        var result = await ReviewAsync(8, "лишаємо", 1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ReviewVersion, Is.EqualTo(2));
    }

    [Test]
    public async Task Refused_while_the_sheet_sync_holds_the_lock()
    {
        harness.Db.LockHeld = true;

        var result = await ReviewAsync(7, null, 0);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Sync_keeps_the_review_and_the_list_shows_supplier_name_and_the_without_supplier_filter()
    {
        await ReviewAsync(7, "нотатка", 0);
        await harness.Sync().RunAsync(CancellationToken.None);

        var all = await ListAsync();
        var linked = all.Items.Single(x => x.MessageId == messageId);
        Assert.That((linked.SupplierId, linked.SupplierName, linked.ReviewNote, linked.ReviewVersion),
            Is.EqualTo(((int?)7, "Текстиль-Контакт", "нотатка", 1)));
        Assert.That(linked.ReviewedBy, Is.EqualTo("buh@brevi.com.ua"));

        var without = await ListAsync(PurchaseInvoiceFilter.WithoutSupplier);
        Assert.That(without.Items.Select(x => x.MessageId), Has.None.EqualTo(messageId));
        Assert.That(without.Total, Is.EqualTo(1));
    }

    [Test]
    public async Task Supplier_options_are_active_suppliers_by_name()
    {
        var result = await new GetPurchaseSuppliersQueryHandler(harness.Suppliers)
            .Handle(new GetPurchaseSuppliersQuery(), CancellationToken.None);

        Assert.That(result.Value.Select(x => x.Name), Is.EqualTo(new[] { "Текстиль-Контакт", "ФОП Бистров" }));
    }

    [Test]
    public void Note_longer_than_the_limit_is_invalid()
    {
        var validator = new ReviewPurchaseInvoiceCommandValidator();
        var command = new ReviewPurchaseInvoiceCommand(messageId, 7, new string('я', 1001), 0, "buh@brevi.com.ua");

        Assert.That(validator.Validate(command).IsValid, Is.False);
        Assert.That(validator.Validate(command with { Note = new string('я', 1000) }).IsValid, Is.True);
    }
}
