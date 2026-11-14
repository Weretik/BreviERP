using Accounting.Domain.Ledger;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;

/// <summary>
/// «Огляд» for <c>Scope</c> (null — both areas) and the inclusive period. One row per currency of the active accounts.
/// <c>NeedsReviewCount</c> is the number of active current operations of «Потребують перевірки», whatever the period;
/// <c>NeedsCategoryCount</c> and <c>NeedsCounterpartyCount</c> split it by reason.
/// </summary>
public sealed record FinanceOverviewResponse(LedgerScope? Scope, DateOnly From, DateOnly To,
    IReadOnlyList<FinanceOverviewCurrencyResponse> Currencies, int NeedsReviewCount, int NeedsCategoryCount,
    int NeedsCounterpartyCount);
