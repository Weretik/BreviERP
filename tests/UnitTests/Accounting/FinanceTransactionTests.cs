using Accounting.Application.Accounts;
using Accounting.Application.Categories;
using Accounting.Application.Contracts;
using Accounting.Application.History;
using Accounting.Application.Journal;
using Accounting.Application.Transactions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Ardalis.Result;
using static UnitTests.Accounting.FinanceMovementFixture;

namespace UnitTests.Accounting;

/// <summary>«Операції»: area and category rules, cancelling, the journal and balances (ACC-001 stage 2).</summary>
public sealed class FinanceTransactionTests
{
    [Test]
    public async Task Operations_move_the_balances_of_their_area_only()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, openingMinor: 1_000_00);
        var materials = await Category(harness, "Матеріали");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        var oldCosts = await Category(harness, "Побут", scope: LedgerScope.LegacyArchive);

        await Spend(harness, Fields(bank.Id, materials.Id, 300_00));
        await Spend(harness, Fields(bank.Id, sales.Id, 500_00, type: CategoryDirection.Income));
        await Spend(harness, Fields(bank.Id, oldCosts.Id, 100_00, Archive), LedgerScope.LegacyArchive);
        var unconfirmed = (await Accounts(harness)).Single().Balance!;

        await new SetCutoverBalanceCommandHandler(harness.Db, harness.Time).Handle(new SetCutoverBalanceCommand(bank.Id,
            2_000_00, true, bank.Version, AccountingTestHarness.Director), default);
        var confirmed = (await Accounts(harness)).Single().Balance!;

        Assert.Multiple(() =>
        {
            Assert.That(unconfirmed.LegacyMinor, Is.EqualTo(900_00));
            Assert.That(unconfirmed.CurrentMinor, Is.Null, "unconfirmed cutover is unknown, not zero");
            Assert.That(unconfirmed.TotalMinor, Is.EqualTo(1_100_00));
            Assert.That(unconfirmed.HasHistory, Is.True);
            Assert.That(confirmed.CurrentMinor, Is.EqualTo(2_200_00));
        });
    }

    [Test]
    public async Task Dates_accounts_and_categories_must_fit_the_area()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        var oldCosts = await Category(harness, "Побут", scope: LedgerScope.LegacyArchive);
        var handler = new CreateFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time);
        Task<Result<FinanceTransactionResponse>> Create(FinanceTransactionFields fields,
            LedgerScope scope = LedgerScope.Current) =>
            handler.Handle(new CreateFinanceTransactionCommand(scope, fields, AccountingTestHarness.Director), default).AsTask();

        AssertRuleBroken(await Create(Fields(bank.Id, materials.Id, 1_00, Archive)), "archive date");
        AssertRuleBroken(await Create(Fields(bank.Id, oldCosts.Id, 1_00)), "archive category");
        AssertRuleBroken(await Create(Fields(bank.Id, sales.Id, 1_00)), "income category");
        AssertRuleBroken(await Create(Fields(bank.Id, oldCosts.Id, 1_00, new DateOnly(2023, 1, 1)),
            LedgerScope.LegacyArchive), "before the opening date");
        AssertRuleBroken(await Create(Fields(bank.Id, materials.Id, 0)), "zero amount");
    }

    [Test]
    public async Task An_archived_account_takes_archive_operations_only()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var oldCosts = await Category(harness, "Побут", scope: LedgerScope.LegacyArchive);
        await new SetFinanceAccountActiveCommandHandler(harness.Db, harness.Time).Handle(
            new SetFinanceAccountActiveCommand(bank.Id, false, bank.Version, AccountingTestHarness.Director), default);

        var archive = await Spend(harness, Fields(bank.Id, oldCosts.Id, 1_00, Archive), LedgerScope.LegacyArchive);

        Assert.That(archive.Scope, Is.EqualTo(LedgerScope.LegacyArchive));
        AssertRuleBroken(await SpendResult(harness, Fields(bank.Id, materials.Id, 1_00)));
    }

    [Test]
    public async Task Update_checks_the_version_and_void_keeps_the_row_but_stops_counting_it()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, openingMinor: 0);
        var materials = await Category(harness, "Матеріали");
        var created = await Spend(harness, Fields(bank.Id, materials.Id, 300_00));
        var update = new UpdateFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time);

        var updated = await update.Handle(new UpdateFinanceTransactionCommand(created.Id,
            Fields(bank.Id, materials.Id, 250_00, description: "Нитки"), created.Version, AccountingTestHarness.Director),
            default);
        var stale = await update.Handle(new UpdateFinanceTransactionCommand(created.Id,
            Fields(bank.Id, materials.Id, 1_00), created.Version, AccountingTestHarness.Director), default);
        var voided = await new VoidFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(created.Id, updated.Value.Version, AccountingTestHarness.Director), default);
        var history = await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Transaction, created.Id), default);

        Assert.Multiple(() =>
        {
            Assert.That(updated.Value.AmountMinor, Is.EqualTo(250_00));
            Assert.That(updated.Value.Description, Is.EqualTo("Нитки"));
            Assert.That(updated.Value.Version, Is.EqualTo(2));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(voided.Value.Status, Is.EqualTo(MovementStatus.Void));
            Assert.That(voided.Value.Version, Is.EqualTo(3));
            Assert.That(history.Value.Select(x => x.Action),
                Is.EqualTo(new[] { AuditAction.Void, AuditAction.Update, AuditAction.Create }));
            Assert.That(history.Value[^1].AfterJson, Does.Contain("Матеріали"));
        });
        var balance = (await Accounts(harness)).Single().Balance!;
        Assert.That(balance.TotalMinor, Is.Zero, "a cancelled operation does not count");
        Assert.That(balance.HasHistory, Is.True, "but the account keeps its history");
        AssertRuleBroken(await update.Handle(new UpdateFinanceTransactionCommand(created.Id,
            Fields(bank.Id, materials.Id, 1_00), voided.Value.Version, AccountingTestHarness.Director), default));
    }

    [Test]
    public async Task Journal_filters_pages_and_totals_the_whole_selection()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var fabric = await Category(harness, "Тканини", parentId: materials.Id);
        var rent = await Category(harness, "Оренда");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        for (var i = 0; i < 55; i++) await Spend(harness, Fields(bank.Id, fabric.Id, 1_00, description: "fabric roll"));
        await Spend(harness, Fields(bank.Id, rent.Id, 5_000_00, new DateOnly(2026, 10, 2)));
        await Spend(harness, Fields(bank.Id, sales.Id, 10_000_00, type: CategoryDirection.Income));
        var voided = await Spend(harness, Fields(bank.Id, rent.Id, 7_00));
        await new VoidFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new VoidFinanceTransactionCommand(voided.Id, voided.Version, AccountingTestHarness.Director), default);
        var handler = new GetFinanceTransactionsQueryHandler(harness.Fresh(), harness.Parties, harness.Parties);
        Task<Result<FinanceTransactionPageResponse>> List(FinanceTransactionFilter f,
            int page = 1, FinanceJournalSort sort = FinanceJournalSort.Date) =>
            handler.Handle(new GetFinanceTransactionsQuery(f, sort, FinanceSortDirection.Desc, page), default).AsTask();
        var all = new FinanceTransactionFilter(LedgerScope.Current, null, null, null, null, null, null, null, null, null, null,
            false);

        var first = (await List(all)).Value;
        var second = (await List(all, 2)).Value;
        var withVoided = (await List(all with { IncludeVoided = true })).Value;
        var byRoot = (await List(all with { CategoryId = materials.Id })).Value;
        var bySearch = (await List(all with { Search = "  FABRIC " })).Value;
        var byPeriod = (await List(all with { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 2) })).Value;
        var byAmount = (await List(all, sort: FinanceJournalSort.Amount)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(first.Total, Is.EqualTo(57));
            Assert.That(first.Items, Has.Count.EqualTo(FinanceJournal.PageSize));
            Assert.That(second.Items, Has.Count.EqualTo(7));
            Assert.That(first.Totals.Single().IncomeMinor, Is.EqualTo(10_000_00));
            Assert.That(first.Totals.Single().ExpenseMinor, Is.EqualTo(5_055_00));
            Assert.That(first.Totals.Single().DifferenceMinor, Is.EqualTo(4_945_00));
            Assert.That(withVoided.Total, Is.EqualTo(58));
            Assert.That(withVoided.Totals.Single().ExpenseMinor, Is.EqualTo(5_055_00), "cancelled rows are not summed");
            Assert.That(byRoot.Total, Is.EqualTo(55), "a root takes its subcategories");
            Assert.That(byRoot.Items[0].CategoryRootName, Is.EqualTo("Матеріали"));
            Assert.That(byRoot.Items[0].SubcategoryName, Is.EqualTo("Тканини"));
            Assert.That(bySearch.Total, Is.EqualTo(55));
            Assert.That(byPeriod.Items.Single().AmountMinor, Is.EqualTo(5_000_00));
            Assert.That(byAmount.Items[0].AmountMinor, Is.EqualTo(10_000_00));
        });
    }

    [Test]
    public async Task A_used_account_can_only_be_archived_and_keeps_its_currency_and_dates()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var oldCosts = await Category(harness, "Побут", scope: LedgerScope.LegacyArchive);
        await Spend(harness, Fields(bank.Id, oldCosts.Id, 1_00, new DateOnly(2024, 3, 1)), LedgerScope.LegacyArchive);
        var update = new UpdateFinanceAccountCommandHandler(harness.Db, harness.Time);

        var delete = await new DeleteFinanceAccountCommandHandler(harness.Db, harness.Time).Handle(
            new DeleteFinanceAccountCommand(bank.Id, bank.Version, AccountingTestHarness.Director), default);
        var earlier = await update.Handle(new UpdateFinanceAccountCommand(bank.Id,
            AccountingTestHarness.Fields("Банк") with { OpeningDate = new DateOnly(2023, 1, 1) }, bank.Version,
            AccountingTestHarness.Director), default);

        Assert.That(delete.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(earlier.IsSuccess, Is.True, "an earlier opening date is fine");
        AssertRuleBroken(await update.Handle(new UpdateFinanceAccountCommand(bank.Id,
            AccountingTestHarness.Fields("Банк") with { Currency = "EUR" }, earlier.Value.Version,
            AccountingTestHarness.Director), default));
        AssertRuleBroken(await update.Handle(new UpdateFinanceAccountCommand(bank.Id,
            AccountingTestHarness.Fields("Банк") with { OpeningDate = new DateOnly(2024, 6, 1) }, earlier.Value.Version,
            AccountingTestHarness.Director), default));
    }

    [Test]
    public async Task Deleting_a_used_category_switches_its_branch_off()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness);
        var materials = await Category(harness, "Матеріали");
        var fabric = await Category(harness, "Тканини", parentId: materials.Id);
        var created = await Spend(harness, Fields(bank.Id, fabric.Id, 1_00));

        var result = await new DeleteFinanceCategoryCommandHandler(harness.Db, harness.Time).Handle(
            new DeleteFinanceCategoryCommand(materials.Id, materials.Version, AccountingTestHarness.Director), default);
        var categories = (await new GetFinanceCategoriesQueryHandler(harness.Fresh())
            .Handle(new GetFinanceCategoriesQuery(LedgerScope.Current), default)).Value;
        var history = await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Category, fabric.Id), default);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(categories.Select(x => x.Active), Is.EqualTo(new[] { false, false }));
            Assert.That(history.Value[0].Action, Is.EqualTo(AuditAction.Deactivate));
        });
        AssertRuleBroken(await SpendResult(harness, Fields(bank.Id, fabric.Id, 1_00)), "switched off");
        var kept = await new UpdateFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new UpdateFinanceTransactionCommand(created.Id, Fields(bank.Id, fabric.Id, 2_00), created.Version,
                AccountingTestHarness.Director), default);
        Assert.That(kept.IsSuccess, Is.True, "an operation keeps its switched-off category");
    }
}
