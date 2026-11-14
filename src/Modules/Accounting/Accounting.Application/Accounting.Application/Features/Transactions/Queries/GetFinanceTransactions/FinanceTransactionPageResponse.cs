using Accounting.Application.Features.Transactions.Shared;
namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

/// <summary>
/// One page of «Операції»: <c>Total</c> rows match the filter, <c>PageSize</c> is fixed at 50. <c>Totals</c> cover the
/// whole filtered selection (not only this page), one row per currency, without cancelled operations.
/// </summary>
public sealed record FinanceTransactionPageResponse(
    IReadOnlyList<FinanceTransactionResponse> Items,
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<FinanceTransactionTotalResponse> Totals);
