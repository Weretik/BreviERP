using Accounting.Domain.Accounts;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Api.Endpoints.Accounts.CreateFinanceAccount;

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
