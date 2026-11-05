using Accounting.Application.Accounts;
using Accounting.Application.Categories;
using Accounting.Application.Contracts;
using Accounting.Application.Transactions;
using Accounting.Application.Transfers;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace UnitTests.Accounting;

/// <summary>Accounts, categories and movements for the stage 2 tests (ACC-001).</summary>
internal static class FinanceMovementFixture
{
    public static readonly DateOnly Current = new(2026, 10, 15);
    public static readonly DateOnly Archive = new(2025, 6, 1);

    public static async Task<FinanceAccountResponse> Account(AccountingTestHarness harness, string name = "Банк",
        long openingMinor = 1_000_00, string currency = "UAH", long? cutoverMinor = null)
    {
        var created = await new CreateFinanceAccountCommandHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceAccountCommand(AccountingTestHarness.Fields(name, openingMinor) with { Currency = currency },
                AccountingTestHarness.Director), default);
        if (cutoverMinor is null) return created.Value;
        return (await new SetCutoverBalanceCommandHandler(harness.Db, harness.Time).Handle(
            new SetCutoverBalanceCommand(created.Value.Id, cutoverMinor, true, created.Value.Version,
                AccountingTestHarness.Director), default)).Value;
    }

    public static async Task<FinanceCategoryResponse> Category(AccountingTestHarness harness, string name,
        CategoryDirection direction = CategoryDirection.Expense, LedgerScope scope = LedgerScope.Current,
        Guid? parentId = null) =>
        (await new CreateFinanceCategoryCommandHandler(harness.Db, harness.Time).Handle(
            new CreateFinanceCategoryCommand(name, direction, scope, parentId, null, 10, AccountingTestHarness.Director),
            default)).Value;

    public static FinanceTransactionFields Fields(Guid accountId, Guid categoryId, long amountMinor, DateOnly? date = null,
        CategoryDirection type = CategoryDirection.Expense, string? description = null) =>
        new(date ?? Current, type, accountId, amountMinor, categoryId, null, null, description, null);

    public static async Task<FinanceTransactionResponse> Spend(AccountingTestHarness harness, FinanceTransactionFields fields,
        LedgerScope scope = LedgerScope.Current) =>
        (await new CreateFinanceTransactionCommandHandler(harness.Db, harness.Parties, harness.Parties, harness.Time).Handle(
            new CreateFinanceTransactionCommand(scope, fields, AccountingTestHarness.Director), default)).Value;

    public static async Task<FinanceTransferResponse> Move(AccountingTestHarness harness, Guid from, Guid to, long amountMinor,
        DateOnly? date = null, LedgerScope scope = LedgerScope.Current) =>
        (await new CreateFinanceTransferCommandHandler(harness.Db, harness.Time).Handle(new CreateFinanceTransferCommand(
            scope, new FinanceTransferFields(date ?? Current, from, to, amountMinor, null), AccountingTestHarness.Director),
            default)).Value;

    public static async Task<IReadOnlyList<FinanceAccountResponse>> Accounts(AccountingTestHarness harness) =>
        (await new GetFinanceAccountsQueryHandler(harness.Fresh()).Handle(new GetFinanceAccountsQuery(true), default)).Value;
}
