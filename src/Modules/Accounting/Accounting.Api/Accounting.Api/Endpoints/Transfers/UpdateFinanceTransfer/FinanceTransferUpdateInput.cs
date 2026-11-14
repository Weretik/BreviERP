using Accounting.Domain.Ledger;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Api.Endpoints.Transfers.UpdateFinanceTransfer;

/// <summary>Changed transfer with the <c>Version</c> the user opened; the area cannot change.</summary>
public sealed record FinanceTransferUpdateInput(
    DateOnly Date,
    Guid FromAccountId,
    Guid ToAccountId,
    long AmountMinor,
    string? Note,
    int Version)
{
    public FinanceTransferFields ToFields() => new(Date, FromAccountId, ToAccountId, AmountMinor, Note);
}
