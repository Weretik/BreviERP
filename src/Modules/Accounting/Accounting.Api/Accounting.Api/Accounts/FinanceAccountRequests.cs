using Accounting.Domain.Accounts;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Api.Accounts;

/// <summary>Account form; money in kopecks, <c>OpeningDate</c> as YYYY-MM-DD.</summary>
public sealed record FinanceAccountInput(
    string? Name,
    FinanceAccountType Type,
    string? Currency,
    string? Iban,
    long OpeningMinor,
    DateOnly OpeningDate,
    string? Note,
    int SortOrder)
{
    public FinanceAccountFields ToFields() => new(Name, Type, Currency, Iban, OpeningMinor, OpeningDate, Note, SortOrder);
}

/// <summary>Account form with the <c>Version</c> the user opened.</summary>
public sealed record FinanceAccountUpdateInput(
    string? Name,
    FinanceAccountType Type,
    string? Currency,
    string? Iban,
    long OpeningMinor,
    DateOnly OpeningDate,
    string? Note,
    int SortOrder,
    int Version)
{
    public FinanceAccountFields ToFields() => new(Name, Type, Currency, Iban, OpeningMinor, OpeningDate, Note, SortOrder);
}

/// <summary>«Залишок на 01.10»: <c>Confirmed</c> false clears the amount and the confirmation.</summary>
public sealed record FinanceAccountCutoverInput(long? BalanceMinor, bool Confirmed, int Version);

/// <summary>The <c>Version</c> the user opened, for «Архівувати» and «Відновити».</summary>
public sealed record FinanceVersionInput(int Version);
