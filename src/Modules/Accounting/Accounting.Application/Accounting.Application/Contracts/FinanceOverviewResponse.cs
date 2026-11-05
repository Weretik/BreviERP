using Accounting.Domain.Ledger;

namespace Accounting.Application.Contracts;

/// <summary>
/// «Огляд» for <c>Scope</c> (null — both areas) and the inclusive period. One row per currency of the active accounts.
/// <c>NeedsReviewCount</c> is the number of active current operations of «Потребують перевірки», whatever the period.
/// </summary>
public sealed record FinanceOverviewResponse(LedgerScope? Scope, DateOnly From, DateOnly To,
    IReadOnlyList<FinanceOverviewCurrencyResponse> Currencies, int NeedsReviewCount);

/// <summary>
/// <c>BalanceMinor</c>: the balances of the active accounts in the chosen area, counted from its start, not limited by
/// the period. For the current books it sums only confirmed accounts; <c>UnconfirmedCutoverCount</c> &gt; 0 means the
/// total is not known yet and the admin shows «Не підтверджено». Income, expense and their difference are active
/// operations of the period.
/// </summary>
public sealed record FinanceOverviewCurrencyResponse(
    string Currency,
    long BalanceMinor,
    int UnconfirmedCutoverCount,
    long IncomeMinor,
    long ExpenseMinor,
    long DifferenceMinor,
    int AccountCount);
