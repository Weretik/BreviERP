using Ardalis.Result;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;
using Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;
using Accounting.Application.Features.Accounts.Queries.GetFinanceAccounts;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;
using Accounting.Application.Features.Categories.Shared;
using Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Transfers.Commands.CreateFinanceTransfer;
using Accounting.Application.Features.Transfers.Shared;

namespace UnitTests.Accounting;

/// <summary>Accounts, categories and movements for the stage 2 tests (ACC-001).</summary>
internal static class FinanceMovementFixture
{
    public static readonly DateOnly Current = new(2026, 10, 15);
    public static readonly DateOnly Archive = new(2025, 6, 1);

    public static async Task<FinanceAccountResponse> Account(AccountingTestHarness harness, string name = "Банк",
        long openingMinor = 1_000_00, string currency = "UAH", long? cutoverMinor = null)
    {
        var created = await new CreateFinanceAccountHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceAccountCommand(AccountingTestHarness.Fields(name, openingMinor) with { Currency = currency },
                AccountingTestHarness.Director), default);
        if (cutoverMinor is null) return created.Value;
        return (await new SetCutoverBalanceHandler(harness.Db, harness.Time).Handle(
            new SetCutoverBalanceCommand(created.Value.Id, cutoverMinor, true, created.Value.Version,
                AccountingTestHarness.Director), default)).Value;
    }

    public static async Task<FinanceCategoryResponse> Category(AccountingTestHarness harness, string name,
        CategoryDirection direction = CategoryDirection.Expense, LedgerScope scope = LedgerScope.Current,
        Guid? parentId = null) =>
        (await new CreateFinanceCategoryHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceCategoryCommand(name, direction, scope, parentId, null, 10, AccountingTestHarness.Director),
            default)).Value;

    public static FinanceTransactionFields Fields(Guid accountId, Guid categoryId, long amountMinor, DateOnly? date = null,
        CategoryDirection type = CategoryDirection.Expense, string? description = null) =>
        new(date ?? Current, type, accountId, amountMinor, categoryId, null, null, description, null);

    public static async Task<FinanceTransactionResponse> Spend(AccountingTestHarness harness, FinanceTransactionFields fields,
        LedgerScope scope = LedgerScope.Current) =>
        (await new CreateFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new CreateFinanceTransactionCommand(scope, fields, AccountingTestHarness.Director), default)).Value;

    public static async Task<Result<FinanceTransactionResponse>> SpendResult(AccountingTestHarness harness,
        FinanceTransactionFields fields, LedgerScope scope = LedgerScope.Current) =>
        await new CreateFinanceTransactionHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new CreateFinanceTransactionCommand(scope, fields, AccountingTestHarness.Director), default);

    public static async Task<Result<FinanceTransferResponse>> MoveResult(AccountingTestHarness harness, Guid from, Guid to,
        long amountMinor, DateOnly? date = null, LedgerScope scope = LedgerScope.Current) =>
        await new CreateFinanceTransferHandler(harness.Db, harness.Time).Handle(new CreateFinanceTransferCommand(
            scope, new FinanceTransferFields(date ?? Current, from, to, amountMinor, null), AccountingTestHarness.Director),
            default);

    /// <summary>A business rule refused the change: the result is invalid, with <paramref name="message"/> if given.</summary>
    public static void AssertRuleBroken(IResult result, string? because = null, string? message = null)
    {
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid), because);
        if (message is not null)
            Assert.That(result.ValidationErrors.Select(x => x.ErrorMessage), Has.Some.Contain(message), because);
    }

    public static async Task<FinanceTransferResponse> Move(AccountingTestHarness harness, Guid from, Guid to, long amountMinor,
        DateOnly? date = null, LedgerScope scope = LedgerScope.Current) =>
        (await new CreateFinanceTransferHandler(harness.Db, harness.Time).Handle(new CreateFinanceTransferCommand(
            scope, new FinanceTransferFields(date ?? Current, from, to, amountMinor, null), AccountingTestHarness.Director),
            default)).Value;

    public static async Task<IReadOnlyList<FinanceAccountResponse>> Accounts(AccountingTestHarness harness) =>
        (await new GetFinanceAccountsHandler(harness.Fresh()).Handle(new GetFinanceAccountsQuery(true), default)).Value;
}
