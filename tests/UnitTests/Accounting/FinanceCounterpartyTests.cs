using Ardalis.Result;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transactions;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;
using Accounting.Application.Features.Categories.Queries.GetFinanceCategories;
using Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;
using Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;
using Accounting.Application.Features.Transactions.Commands.PromoteFinanceOneoff;
using Accounting.Application.Features.Transactions.Commands.ReviewFinanceTransaction;
using Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;
using Accounting.Application.Features.Transactions.Shared;

namespace UnitTests.Accounting;

/// <summary>«Контрагенти» and «Потребують перевірки» (ACC-001 stage 3).</summary>
public sealed class FinanceCounterpartyTests
{
    private static readonly Guid ClientId = Guid.Parse("0195f0a0-0000-7000-8000-000000000001");

    [Test]
    public async Task An_unchecked_counterparty_sends_its_operations_to_review_until_it_is_confirmed()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var rent = await Category(harness, "Оренда");
        var landlord = await Counterparty(harness, "ФОП Орендодавець");
        var operation = await Spend(harness, Fields(bank.Id, rent.Id, 5_000_00) with { Counterparty = Link(landlord) });
        var waiting = await Review(harness);
        var overview = await Overview(harness);

        var verified = await new VerifyFinanceCounterpartyHandler(harness.Db, harness.Time).Handle(
            new VerifyFinanceCounterpartyCommand(landlord.Id, landlord.Version, AccountingTestHarness.Director), default);
        var after = (await Review(harness, onlyReview: false)).Items.Single();

        Assert.Multiple(() =>
        {
            Assert.That(operation.Verification, Is.EqualTo(VerificationStatus.NeedsCounterpartyReview));
            Assert.That(operation.Counterparty, Is.EqualTo(new FinanceCounterpartyRefResponse(CounterpartyKind.Financial,
                landlord.Id.ToString("D"), "ФОП Орендодавець", true, false, false)));
            Assert.That(waiting.Items.Select(x => x.Id), Is.EqualTo(new[] { operation.Id }));
            Assert.That(overview.NeedsReviewCount, Is.EqualTo(1));
            Assert.That(overview.NeedsCounterpartyCount, Is.EqualTo(1));
            Assert.That(overview.NeedsCategoryCount, Is.Zero);
            Assert.That(verified.Value.Verified, Is.True);
            Assert.That(verified.Value.OperationCount, Is.EqualTo(1));
            Assert.That(after.Verification, Is.EqualTo(VerificationStatus.Verified));
            Assert.That(after.Version, Is.EqualTo(operation.Version), "the operation itself did not change");
            Assert.That(after.Counterparty!.Verified, Is.True);
        });
    }

    [Test]
    public async Task Clients_and_suppliers_are_linked_by_id_and_never_need_a_review()
    {
        using var harness = new AccountingTestHarness();
        harness.Parties.Clients.Add(new AccountingParty(ClientId.ToString("D"), "ТОВ Клієнт", "12345678", true));
        harness.Parties.Clients.Add(new AccountingParty(Guid.CreateVersion7().ToString("D"), "Архівний", "", false));
        harness.Parties.Suppliers.Add(new AccountingParty("7", "Тканини Плюс", "", true));
        var bank = await Account(harness);
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        var fabric = await Category(harness, "Тканини");

        var income = await Spend(harness, Fields(bank.Id, sales.Id, 1_000_00,
            type: CategoryDirection.Income) with
        {
            Counterparty = new FinanceCounterpartyLinkInput(CounterpartyKind.Client, ClientId.ToString("N")),
        });
        var expense = await Spend(harness, Fields(bank.Id, fabric.Id, 300_00) with
        {
            Counterparty = new FinanceCounterpartyLinkInput(CounterpartyKind.Supplier, "007"),
        });
        var create = new CreateFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time);
        var archived = harness.Parties.Clients[1];
        var bySupplier = (await new GetFinanceTransactionsHandler(harness.Fresh(), harness.Parties, harness.Parties)
            .Handle(new GetFinanceTransactionsQuery(Filter() with { Counterparty = new FinanceCounterpartyLinkInput(CounterpartyKind.Supplier, "7") },
                FinanceJournalSort.Date, FinanceSortDirection.Desc, 1), default)).Value;

        AssertDomain(await create.Handle(new CreateFinanceTransactionCommand(LedgerScope.Current,
            Fields(bank.Id, fabric.Id, 1_00) with { Counterparty = new(CounterpartyKind.Client, archived.Id) },
            AccountingTestHarness.Director), default).AsTask(), "в архіві");
        AssertDomain(await create.Handle(new CreateFinanceTransactionCommand(LedgerScope.Current,
            Fields(bank.Id, fabric.Id, 1_00) with { Counterparty = new(CounterpartyKind.Supplier, "7"), OneoffName = "Іван" },
            AccountingTestHarness.Director), default).AsTask(), "не обидва");
        var gone = await create.Handle(new CreateFinanceTransactionCommand(LedgerScope.Current,
            Fields(bank.Id, fabric.Id, 1_00) with { Counterparty = new(CounterpartyKind.Supplier, "99") },
            AccountingTestHarness.Director), default);

        Assert.Multiple(() =>
        {
            Assert.That(income.Counterparty!.Name, Is.EqualTo("ТОВ Клієнт"));
            Assert.That(income.Counterparty.Id, Is.EqualTo(ClientId.ToString("D")), "the id is kept in one spelling");
            Assert.That(expense.Counterparty!.Id, Is.EqualTo("7"));
            Assert.That(new[] { income.Verification, expense.Verification }, Is.All.EqualTo(VerificationStatus.Verified));
            Assert.That(bySupplier.Items.Select(x => x.Id), Is.EqualTo(new[] { expense.Id }));
            Assert.That(gone.Status, Is.EqualTo(Ardalis.Result.ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task Review_sets_the_category_and_counterparty_and_needs_a_confirmed_one()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var rent = await Category(harness, "Оренда");
        var office = await Category(harness, "Офіс", parentId: rent.Id);
        var unchecked_ = await Counterparty(harness, "Новий");
        var checked_ = await Counterparty(harness, "Перевірений", verified: true);
        var operation = await Spend(harness, Fields(bank.Id, rent.Id, 100_00) with { Counterparty = Link(unchecked_) });
        var handler = new ReviewFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time);

        AssertDomain(await handler.Handle(new ReviewFinanceTransactionCommand(operation.Id, office.Id,
            Link(unchecked_), null, operation.Version, AccountingTestHarness.Director), default).AsTask(), "Спершу підтвердіть");
        var reviewed = await handler.Handle(new ReviewFinanceTransactionCommand(operation.Id, office.Id, Link(checked_), null,
            operation.Version, AccountingTestHarness.Director), default);
        var history = await new GetFinanceHistoryHandler(harness.Fresh()).Handle(
            new GetFinanceHistoryQuery(AuditEntity.Transaction, operation.Id), default);

        Assert.Multiple(() =>
        {
            Assert.That(reviewed.Value.Verification, Is.EqualTo(VerificationStatus.Verified));
            Assert.That(reviewed.Value.SubcategoryName, Is.EqualTo("Офіс"));
            Assert.That(reviewed.Value.Counterparty!.Name, Is.EqualTo("Перевірений"));
            Assert.That(reviewed.Value.Version, Is.EqualTo(operation.Version + 1));
            Assert.That(reviewed.Value.AmountMinor, Is.EqualTo(100_00));
            Assert.That(history.Value.Select(x => x.Action).First(), Is.EqualTo(AuditAction.Review));
        });
    }

    [Test]
    public async Task Review_can_remember_the_category_for_the_next_operations_of_the_counterparty()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var delivery = await Category(harness, "Доставка");
        var courier = await Counterparty(harness, "Делівері", verified: true);
        var operation = await Spend(harness, Fields(bank.Id, delivery.Id, 192_00) with { Counterparty = Link(courier) });
        var handler = new ReviewFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time);

        var reviewed = await handler.Handle(new ReviewFinanceTransactionCommand(operation.Id, delivery.Id, Link(courier),
            null, operation.Version, AccountingTestHarness.Director, RememberCategory: true), default);
        var remembered = (await List(harness, FinanceCounterpartyTab.Financial)).Financial.Single();
        var history = await new GetFinanceHistoryHandler(harness.Fresh()).Handle(
            new GetFinanceHistoryQuery(AuditEntity.Counterparty, courier.Id), default);

        Assert.Multiple(() =>
        {
            Assert.That(reviewed.IsSuccess, Is.True);
            Assert.That(remembered.DefaultCategoryId, Is.EqualTo(delivery.Id));
            Assert.That(remembered.AutoApplyCategory, Is.True);
            Assert.That(remembered.Version, Is.EqualTo(courier.Version + 1));
            Assert.That(remembered.OperationCount, Is.EqualTo(1));
            Assert.That(remembered.CurrentTurnoverMinor, Is.EqualTo(-192_00), "an expense lowers the turnover");
            Assert.That(history.Value.Select(x => x.Action).First(), Is.EqualTo(AuditAction.Update));
        });
    }

    [Test]
    public async Task A_one_off_name_becomes_a_checked_counterparty_on_request()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var taxes = await Category(harness, "Податки");
        var first = await Spend(harness, Fields(bank.Id, taxes.Id, 50_00) with { OneoffName = "  ДПС   Київ " });
        var second = await Spend(harness, Fields(bank.Id, taxes.Id, 60_00) with { OneoffName = "ДПС Київ" });
        var oneoffs = await List(harness, FinanceCounterpartyTab.Oneoffs);

        var promoted = await new PromoteFinanceOneoffHandler(harness.Db, harness.Time).Handle(
            new PromoteFinanceOneoffCommand(first.Id, first.Version, AccountingTestHarness.Director), default);
        var financial = await List(harness, FinanceCounterpartyTab.Financial);

        Assert.Multiple(() =>
        {
            Assert.That(first.OneoffName, Is.EqualTo("ДПС Київ"));
            Assert.That(oneoffs.Oneoffs.Select(x => x.Id), Is.EquivalentTo(new[] { first.Id, second.Id }));
            Assert.That(oneoffs.Counts.Oneoffs, Is.EqualTo(2));
            Assert.That(promoted.Value.OneoffName, Is.Empty);
            Assert.That(promoted.Value.Counterparty!.Name, Is.EqualTo("ДПС Київ"));
            Assert.That(promoted.Value.Counterparty.Verified, Is.True);
            Assert.That(financial.Financial.Single().OperationCount, Is.EqualTo(1), "the other one-off stays as it was");
            Assert.That(financial.Counts.Oneoffs, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Merging_moves_the_operations_and_keeps_the_names_as_other_spellings()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var rent = await Category(harness, "Оренда");
        var duplicate = await Counterparty(harness, "Укртелеком ПАТ", verified: true);
        var main = await Counterparty(harness, "Укртелеком", verified: true);
        var operation = await Spend(harness, Fields(bank.Id, rent.Id, 10_00) with { Counterparty = Link(duplicate) });

        var merged = await new MergeFinanceCounterpartyHandler(harness.Db, harness.Time).Handle(
            new MergeFinanceCounterpartyCommand(duplicate.Id, duplicate.Version, main.Id, main.Version,
                AccountingTestHarness.Director), default);
        var moved = (await Review(harness, onlyReview: false)).Items.Single();
        var all = await List(harness, FinanceCounterpartyTab.Financial, includeArchived: true);
        var update = new UpdateFinanceCounterpartyHandler(harness.Db, harness.Time);
        var source = all.Financial.Single(x => x.Id == duplicate.Id);

        var byOldName = (await List(harness, FinanceCounterpartyTab.Financial, search: "пат")).Financial;
        AssertDomain(await update.Handle(new UpdateFinanceCounterpartyCommand(source.Id,
            Details("Інше"), source.Version, AccountingTestHarness.Director), default).AsTask(), "об'єднано");

        Assert.Multiple(() =>
        {
            Assert.That(merged.Value.Aliases, Is.EqualTo(new[] { "Укртелеком ПАТ" }));
            Assert.That(merged.Value.OperationCount, Is.EqualTo(1));
            Assert.That(moved.Id, Is.EqualTo(operation.Id));
            Assert.That(moved.Counterparty!.Id, Is.EqualTo(main.Id.ToString("D")));
            Assert.That(source.Active, Is.False);
            Assert.That(source.MergedIntoId, Is.EqualTo(main.Id));
            Assert.That(byOldName.Single().Id, Is.EqualTo(main.Id), "the old name finds the main record");
        });
    }

    [Test]
    public async Task Fields_are_cleaned_and_auto_category_needs_a_checked_counterparty()
    {
        using var harness = new AccountingTestHarness();
        var rent = await Category(harness, "Оренда");
        var archiveCategory = await Category(harness, "Побут", scope: LedgerScope.LegacyArchive);
        var create = new CreateFinanceCounterpartyHandler(harness.Db, harness.Time);
        var created = await create.Handle(new CreateFinanceCounterpartyCommand(new FinanceCounterpartyFields(
            " ТОВ  Ромашка ", "1234 5678", "ua21 3052-9900", "ROMASHKA TOV",
            ["Ромашка", "ромашка", "ТОВ Ромашка", " "], rent.Id, true, null), true, AccountingTestHarness.Director), default);

        AssertDomain(await create.Handle(new CreateFinanceCounterpartyCommand(Details("Б") with
        {
            DefaultCategoryId = rent.Id, AutoApplyCategory = true,
        }, false, AccountingTestHarness.Director), default).AsTask(), "підтвердженого");
        AssertDomain(await create.Handle(new CreateFinanceCounterpartyCommand(Details("В") with
        {
            DefaultCategoryId = archiveCategory.Id,
        }, false, AccountingTestHarness.Director), default).AsTask(), "поточного обліку");
        AssertDomain(await create.Handle(new CreateFinanceCounterpartyCommand(Details("Г") with { Edrpou = "12-AB" },
            false, AccountingTestHarness.Director), default).AsTask(), "лише цифри");

        Assert.Multiple(() =>
        {
            Assert.That(created.Value.Name, Is.EqualTo("ТОВ Ромашка"));
            Assert.That(created.Value.Edrpou, Is.EqualTo("12345678"));
            Assert.That(created.Value.Iban, Is.EqualTo("UA2130529900"));
            Assert.That(created.Value.Aliases, Is.EqualTo(new[] { "Ромашка" }));
            Assert.That(created.Value.DefaultCategoryRootName, Is.EqualTo("Оренда"));
        });
    }

    [Test]
    public async Task Tabs_count_with_the_same_search_and_choices_mix_every_kind()
    {
        using var harness = new AccountingTestHarness();
        harness.Parties.Clients.Add(new AccountingParty(ClientId.ToString("D"), "Ромашка клієнт", "1", true));
        harness.Parties.Suppliers.Add(new AccountingParty("3", "Ромашка постачальник", "", true));
        harness.Parties.Suppliers.Add(new AccountingParty("4", "Інший", "", true));
        var archived = await Counterparty(harness, "Ромашка стара", verified: true);
        await new SetFinanceCounterpartyActiveHandler(harness.Db, harness.Time).Handle(
            new SetFinanceCounterpartyActiveCommand(archived.Id, false, archived.Version, AccountingTestHarness.Director), default);
        await Counterparty(harness, "Ромашка фін");

        var page = await List(harness, FinanceCounterpartyTab.Suppliers, search: "ромашка");
        var choices = (await new GetFinanceCounterpartyChoicesHandler(harness.Fresh(), harness.Parties, harness.Parties)
            .Handle(new GetFinanceCounterpartyChoicesQuery("Ромашка"), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(page.Counts, Is.EqualTo(new FinanceCounterpartyCountsResponse(1, 1, 1, 0)));
            Assert.That(page.Parties.Single().Name, Is.EqualTo("Ромашка постачальник"));
            Assert.That(choices.Select(x => (x.Kind, x.Name)), Is.EqualTo(new[]
            {
                (CounterpartyKind.Financial, "Ромашка фін"),
                (CounterpartyKind.Client, "Ромашка клієнт"),
                (CounterpartyKind.Supplier, "Ромашка постачальник"),
            }), "archived ones are not offered");
            Assert.That(choices[0].Verified, Is.False);
        });
    }

    [Test]
    public async Task A_default_category_of_a_counterparty_is_switched_off_instead_of_deleted()
    {
        using var harness = new AccountingTestHarness();
        var rent = await Category(harness, "Оренда");
        await new CreateFinanceCounterpartyHandler(harness.Db, harness.Time).Handle(new CreateFinanceCounterpartyCommand(
            Details("Орендодавець") with { DefaultCategoryId = rent.Id }, false, AccountingTestHarness.Director), default);

        var deleted = await new DeleteFinanceCategoryHandler(harness.Db, harness.Time).Handle(
            new DeleteFinanceCategoryCommand(rent.Id, rent.Version, AccountingTestHarness.Director), default);
        var categories = (await new GetFinanceCategoriesHandler(harness.Fresh()).Handle(
            new GetFinanceCategoriesQuery(null), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(categories.Single().Active, Is.False);
        });
    }

    private static FinanceCounterpartyFields Details(string name) => new(name, null, null, null, null, null, false, null);

    private static FinanceCounterpartyLinkInput Link(FinanceCounterpartyResponse counterparty) =>
        new(CounterpartyKind.Financial, counterparty.Id.ToString("D"));

    private static async Task<FinanceCounterpartyResponse> Counterparty(AccountingTestHarness harness, string name,
        bool verified = false) =>
        (await new CreateFinanceCounterpartyHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceCounterpartyCommand(Details(name), verified, AccountingTestHarness.Director), default)).Value;

    private static FinanceTransactionFilter Filter() =>
        new(LedgerScope.Current, null, null, null, null, null, null, null, null, null, null, false);

    private static async Task<FinanceTransactionPageResponse> Review(AccountingTestHarness harness, bool onlyReview = true) =>
        (await new GetFinanceTransactionsHandler(harness.Fresh(), harness.Parties, harness.Parties).Handle(
            new GetFinanceTransactionsQuery(Filter() with { NeedsReview = onlyReview }, FinanceJournalSort.Date,
                FinanceSortDirection.Desc, 1), default)).Value;

    private static async Task<FinanceOverviewResponse> Overview(AccountingTestHarness harness) =>
        (await new GetFinanceOverviewHandler(harness.Fresh()).Handle(
            new GetFinanceOverviewQuery(LedgerScope.Current, Current, Current), default)).Value;

    private static async Task<FinanceCounterpartyPageResponse> List(AccountingTestHarness harness, FinanceCounterpartyTab tab,
        string? search = null, bool includeArchived = false) =>
        (await new GetFinanceCounterpartiesHandler(harness.Fresh(), harness.Parties, harness.Parties).Handle(
            new GetFinanceCounterpartiesQuery(tab, search, includeArchived, 1), default)).Value;

    private static void AssertDomain(IResult result, string message) => AssertRuleBroken(result, message: message);
}
