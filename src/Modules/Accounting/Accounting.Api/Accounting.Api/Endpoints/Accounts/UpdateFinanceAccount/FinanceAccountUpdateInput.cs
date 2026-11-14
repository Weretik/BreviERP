using Accounting.Domain.Accounts;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Api.Endpoints.Accounts.UpdateFinanceAccount;

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
