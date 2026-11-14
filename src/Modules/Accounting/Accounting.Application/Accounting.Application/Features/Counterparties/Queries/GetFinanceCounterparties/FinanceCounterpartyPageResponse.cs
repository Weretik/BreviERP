using Accounting.Domain.Counterparties;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;

/// <summary>
/// One page of «Контрагенти» for one tab: <c>Financial</c> for «Фінансові», <c>Parties</c> for «Клієнти» and
/// «Постачальники», <c>Oneoffs</c> — operations with a one-off name — for «Разові». <c>Counts</c> are per tab with the
/// same search. 50 rows per page.
/// </summary>
public sealed record FinanceCounterpartyPageResponse(
    FinanceCounterpartyTab Tab,
    IReadOnlyList<FinanceCounterpartyResponse> Financial,
    IReadOnlyList<FinancePartyResponse> Parties,
    IReadOnlyList<FinanceTransactionResponse> Oneoffs,
    int Total,
    int Page,
    int PageSize,
    FinanceCounterpartyCountsResponse Counts);
