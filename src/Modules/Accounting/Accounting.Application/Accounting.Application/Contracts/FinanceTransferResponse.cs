using Accounting.Application.Lookup;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Contracts;

/// <summary>A row of «Перекази»; <c>AmountMinor</c> is positive kopecks moved from one account to the other.</summary>
public sealed record FinanceTransferResponse(
    Guid Id,
    DateOnly Date,
    Guid FromAccountId,
    string FromAccountName,
    Guid ToAccountId,
    string ToAccountName,
    long AmountMinor,
    string Currency,
    string Note,
    MovementSource Source,
    LedgerScope Scope,
    MovementStatus Status,
    int Version,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    internal static FinanceTransferResponse From(FinanceTransfer x, FinanceNames names) => new(x.Id.Value, x.Date,
        x.FromAccountId.Value, names.Account(x.FromAccountId), x.ToAccountId.Value, names.Account(x.ToAccountId),
        x.AmountMinor, x.Currency, x.Note, x.Source, x.Scope, x.Status, x.Version, x.CreatedBy, x.CreatedAt, x.UpdatedAt);
}
