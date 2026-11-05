using Accounting.Domain.Accounts;

namespace Accounting.Application.Contracts;

/// <summary>
/// An account of «Рахунки і каси». Money is in kopecks; <c>Version</c> is sent back with the next change.
/// <c>Balance</c> is counted on every read and is null only inside «Журнал змін» snapshots.
/// </summary>
public sealed record FinanceAccountResponse(
    Guid Id,
    string Name,
    FinanceAccountType Type,
    string Currency,
    string Iban,
    long OpeningMinor,
    DateOnly OpeningDate,
    long? CutoverBalanceMinor,
    bool CutoverConfirmed,
    DateTimeOffset? CutoverConfirmedAt,
    string? CutoverConfirmedBy,
    string Note,
    bool Active,
    int SortOrder,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    FinanceAccountBalanceResponse? Balance)
{
    public static FinanceAccountResponse From(FinanceAccount x, FinanceAccountBalanceResponse? balance) => new(x.Id.Value,
        x.Name, x.Type, x.Currency, x.Iban, x.OpeningMinor, x.OpeningDate, x.CutoverBalanceMinor, x.CutoverConfirmed,
        x.CutoverConfirmedAt, x.CutoverConfirmedBy, x.Note, x.Active, x.SortOrder, x.Version, x.CreatedAt, x.UpdatedAt,
        balance);
}
