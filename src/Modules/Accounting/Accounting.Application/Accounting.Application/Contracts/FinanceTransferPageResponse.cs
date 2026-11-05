namespace Accounting.Application.Contracts;

/// <summary>
/// One page of «Перекази»: <c>Total</c> rows match the filter, <c>PageSize</c> is fixed at 50. <c>Totals</c> sum the
/// active transfers of the whole filtered selection, one row per currency (each transfer counted once).
/// </summary>
public sealed record FinanceTransferPageResponse(
    IReadOnlyList<FinanceTransferResponse> Items,
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<FinanceTransferTotalResponse> Totals);

public sealed record FinanceTransferTotalResponse(string Currency, long AmountMinor);
