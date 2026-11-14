namespace Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;

public sealed record FinanceTransferTotalResponse(string Currency, long AmountMinor);
