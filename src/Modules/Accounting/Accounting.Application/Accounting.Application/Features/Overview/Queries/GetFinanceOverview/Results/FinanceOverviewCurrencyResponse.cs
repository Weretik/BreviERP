using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;

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
