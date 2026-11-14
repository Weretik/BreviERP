using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Ardalis.Result;
using static UnitTests.Accounting.FinanceMovementFixture;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;
using Accounting.Application.Features.Transfers.Commands.UpdateFinanceTransfer;
using Accounting.Application.Features.Transfers.Commands.VoidFinanceTransfer;
using Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;
using Accounting.Application.Features.Transfers.Shared;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;

namespace UnitTests.Accounting.Ledger;

/// <summary>«Перекази» and «Огляд» (ACC-001 stage 2).</summary>
public sealed class FinanceTransferTests
{
    [Test]
    public async Task A_transfer_moves_money_between_accounts_without_changing_their_sum()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, "Банк", 1_000_00, cutoverMinor: 1_000_00);
        var cash = await Account(harness, "Каса", 0, cutoverMinor: 0);

        await Move(harness, bank.Id, cash.Id, 400_00);
        var accounts = await Accounts(harness);

        Assert.Multiple(() =>
        {
            Assert.That(accounts.Single(x => x.Id == bank.Id).Balance!.CurrentMinor, Is.EqualTo(600_00));
            Assert.That(accounts.Single(x => x.Id == cash.Id).Balance!.CurrentMinor, Is.EqualTo(400_00));
            Assert.That(accounts.Sum(x => x.Balance!.TotalMinor), Is.EqualTo(1_000_00));
        });
    }

    [Test]
    public async Task Transfer_needs_two_accounts_of_one_currency()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, "Банк");
        var euro = await Account(harness, "Євро", currency: "EUR");

        AssertRuleBroken(await MoveResult(harness, bank.Id, bank.Id, 1_00));
        AssertRuleBroken(await MoveResult(harness, bank.Id, euro.Id, 1_00));
        AssertRuleBroken(await MoveResult(harness, bank.Id, euro.Id, 1_00, Archive), "archive date in current");
    }

    [Test]
    public async Task Journal_filters_by_either_account_and_totals_active_transfers_once()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, "Банк");
        var cash = await Account(harness, "Каса");
        var card = await Account(harness, "Картка");
        await Move(harness, bank.Id, cash.Id, 100_00);
        await Move(harness, cash.Id, card.Id, 30_00);
        var voided = await Move(harness, bank.Id, card.Id, 5_00);
        var voidResult = await new VoidFinanceTransferHandler(harness.Db, harness.Time).Handle(
            new VoidFinanceTransferCommand(voided.Id, voided.Version, AccountingTestHarness.Director), default);
        var stale = await new UpdateFinanceTransferHandler(harness.Db, harness.Time).Handle(
            new UpdateFinanceTransferCommand(voided.Id, new FinanceTransferFields(Current, bank.Id, card.Id, 6_00, null),
                voided.Version, AccountingTestHarness.Director), default);
        var handler = new GetFinanceTransfersHandler(harness.Fresh());
        var all = new FinanceTransferFilter(LedgerScope.Current, null, null, null, null, null, null, null, false);

        var byCash = (await handler.Handle(new GetFinanceTransfersQuery(all with { AccountId = cash.Id },
            FinanceJournalSort.Date, FinanceSortDirection.Desc, 1), default)).Value;
        var withVoided = (await handler.Handle(new GetFinanceTransfersQuery(all with { IncludeVoided = true },
            FinanceJournalSort.Amount, FinanceSortDirection.Asc, 1), default)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(voidResult.Value.Status, Is.EqualTo(MovementStatus.Void));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(byCash.Total, Is.EqualTo(2));
            Assert.That(byCash.Totals.Single().AmountMinor, Is.EqualTo(130_00));
            Assert.That(withVoided.Total, Is.EqualTo(3));
            Assert.That(withVoided.Items[0].AmountMinor, Is.EqualTo(5_00));
            Assert.That(withVoided.Items[0].FromAccountName, Is.EqualTo("Банк"));
            Assert.That(withVoided.Totals.Single().AmountMinor, Is.EqualTo(130_00));
        });
    }

    [Test]
    public async Task Overview_sums_active_accounts_per_currency_and_counts_unconfirmed_ones()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Account(harness, "Банк", 1_000_00, cutoverMinor: 2_000_00);
        await Account(harness, "Каса", 500_00);
        await Account(harness, "Євро", 10_00, "EUR");
        var materials = await Category(harness, "Матеріали");
        var fabric = await Category(harness, "Тканини", parentId: materials.Id);
        var rent = await Category(harness, "Оренда");
        var sales = await Category(harness, "Продажі", CategoryDirection.Income);
        await Spend(harness, Fields(bank.Id, materials.Id, 100_00));
        await Spend(harness, Fields(bank.Id, fabric.Id, 120_00));
        await Spend(harness, Fields(bank.Id, rent.Id, 80_00));
        await Spend(harness, Fields(bank.Id, sales.Id, 800_00, new DateOnly(2026, 11, 2), CategoryDirection.Income));
        var handler = new GetFinanceOverviewHandler(harness.Fresh());
        Task<Result<FinanceOverviewResponse>> Get(LedgerScope? scope, DateOnly from) =>
            handler.Handle(new GetFinanceOverviewQuery(scope, from, new DateOnly(2026, 11, 5)), default).AsTask();

        var current = (await Get(LedgerScope.Current, new DateOnly(2026, 10, 1))).Value;
        var november = (await Get(LedgerScope.Current, new DateOnly(2026, 11, 1))).Value;
        var all = (await Get(null, new DateOnly(2026, 10, 1))).Value;

        var uah = current.Currencies[0];
        Assert.Multiple(() =>
        {
            Assert.That(current.Currencies.Select(x => x.Currency), Is.EqualTo(new[] { "UAH", "EUR" }));
            Assert.That(uah.BalanceMinor, Is.EqualTo(2_500_00), "only the confirmed account is summed");
            Assert.That(uah.UnconfirmedCutoverCount, Is.EqualTo(1));
            Assert.That(uah.IncomeMinor, Is.EqualTo(800_00));
            Assert.That(uah.ExpenseMinor, Is.EqualTo(300_00));
            Assert.That(uah.DifferenceMinor, Is.EqualTo(500_00));
            Assert.That((uah.IncomeCount, uah.ExpenseCount), Is.EqualTo((1, 3)));
            Assert.That(uah.TopExpenses.Select(x => (x.Name, x.AmountMinor)), Is.EqualTo(new[]
            {
                ("Матеріали", 220_00L), ("Оренда", 80_00L),
            }), "subcategories fold into their root, largest first");
            Assert.That((uah.OtherExpenseMinor, uah.OtherExpenseCategoryCount), Is.EqualTo((0L, 0)));
            Assert.That(november.Currencies[0].ExpenseMinor, Is.Zero);
            Assert.That(november.Currencies[0].BalanceMinor, Is.EqualTo(2_500_00), "balances ignore the period");
            Assert.That(all.Currencies[0].BalanceMinor, Is.EqualTo(2_000_00), "opening balances plus every movement");
            Assert.That(all.Currencies[0].UnconfirmedCutoverCount, Is.Zero);
        });
    }
}
