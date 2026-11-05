using Accounting.Domain.Ledger;

namespace Accounting.Application.Contracts;

/// <summary>
/// «Огляд» for <c>Scope</c> (null — both areas) and the inclusive period. One row per currency of the active accounts.
/// <c>NeedsReviewCount</c> is the number of active current operations of «Потребують перевірки», whatever the period;
/// <c>NeedsCategoryCount</c> and <c>NeedsCounterpartyCount</c> split it by reason.
/// </summary>
public sealed record FinanceOverviewResponse(LedgerScope? Scope, DateOnly From, DateOnly To,
    IReadOnlyList<FinanceOverviewCurrencyResponse> Currencies, int NeedsReviewCount, int NeedsCategoryCount,
    int NeedsCounterpartyCount);

/// <summary>
/// <c>BalanceMinor</c>: the balances of the active accounts in the chosen area, counted from its start, not limited by
/// the period. For the current books it sums only confirmed accounts; <c>UnconfirmedCutoverCount</c> &gt; 0 means the
/// total is not known yet and the admin shows «Не підтверджено». Income, expense, their difference and counts are active
/// operations of the period. <c>TopExpenses</c> are the largest expense root categories of the period (at most
/// <see cref="FinanceOverviewLimits.TopExpenses"/>); <c>OtherExpenseMinor</c> sums the rest, operations without a
/// category included, over <c>OtherExpenseCategoryCount</c> groups.
/// </summary>
public sealed record FinanceOverviewCurrencyResponse(
    string Currency,
    long BalanceMinor,
    int UnconfirmedCutoverCount,
    long IncomeMinor,
    long ExpenseMinor,
    long DifferenceMinor,
    int AccountCount,
    int IncomeCount,
    int ExpenseCount,
    IReadOnlyList<FinanceOverviewCategoryResponse> TopExpenses,
    long OtherExpenseMinor,
    int OtherExpenseCategoryCount);

/// <summary>An expense root category of «Куди пішли гроші»; <c>CategoryId</c> is null for operations without one.</summary>
public sealed record FinanceOverviewCategoryResponse(Guid? CategoryId, string Name, long AmountMinor);

public static class FinanceOverviewLimits
{
    public const int TopExpenses = 5;
}
