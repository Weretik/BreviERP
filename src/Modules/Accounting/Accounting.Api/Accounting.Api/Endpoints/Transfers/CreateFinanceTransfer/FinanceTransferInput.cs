using Accounting.Domain.Ledger;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Api.Endpoints.Transfers.CreateFinanceTransfer;

/// <summary>
/// New transfer: the area of the screen, kopecks, <c>Date</c> as YYYY-MM-DD. <c>RequestId</c> is made when the form
/// opens: a repeated «Зберегти» returns the saved transfer.
/// </summary>
public sealed record FinanceTransferInput(
    LedgerScope Scope,
    DateOnly Date,
    Guid FromAccountId,
    Guid ToAccountId,
    long AmountMinor,
    string? Note,
    Guid? RequestId = null)
{
    public FinanceTransferFields ToFields() => new(Date, FromAccountId, ToAccountId, AmountMinor, Note);
}
