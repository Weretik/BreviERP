using Accounting.Application.Transactions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Transactions;

/// <summary>New income or expense: the area of the screen, kopecks, <c>Date</c> as YYYY-MM-DD.</summary>
public sealed record FinanceTransactionInput(
    LedgerScope Scope,
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    long AmountMinor,
    Guid CategoryId,
    string? OneoffName,
    string? Description,
    string? Comment)
{
    public FinanceTransactionFields ToFields() =>
        new(Date, Type, AccountId, AmountMinor, CategoryId, OneoffName, Description, Comment);
}

/// <summary>Changed income or expense with the <c>Version</c> the user opened; the area cannot change.</summary>
public sealed record FinanceTransactionUpdateInput(
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    long AmountMinor,
    Guid CategoryId,
    string? OneoffName,
    string? Description,
    string? Comment,
    int Version)
{
    public FinanceTransactionFields ToFields() =>
        new(Date, Type, AccountId, AmountMinor, CategoryId, OneoffName, Description, Comment);
}
