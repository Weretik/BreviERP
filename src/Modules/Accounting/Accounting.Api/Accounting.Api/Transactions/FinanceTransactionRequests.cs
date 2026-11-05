using Accounting.Application.Transactions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Transactions;

/// <summary>
/// New income or expense: the area of the screen, kopecks, <c>Date</c> as YYYY-MM-DD. <c>Counterparty</c> (picked from
/// the list) and <c>OneoffName</c> (typed for this operation only) cannot both be set.
/// </summary>
public sealed record FinanceTransactionInput(
    LedgerScope Scope,
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    long AmountMinor,
    Guid CategoryId,
    FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName,
    string? Description,
    string? Comment)
{
    public FinanceTransactionFields ToFields() =>
        new(Date, Type, AccountId, AmountMinor, CategoryId, Counterparty, OneoffName, Description, Comment);
}

/// <summary>Changed income or expense with the <c>Version</c> the user opened; the area cannot change.</summary>
public sealed record FinanceTransactionUpdateInput(
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    long AmountMinor,
    Guid CategoryId,
    FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName,
    string? Description,
    string? Comment,
    int Version)
{
    public FinanceTransactionFields ToFields() =>
        new(Date, Type, AccountId, AmountMinor, CategoryId, Counterparty, OneoffName, Description, Comment);
}

/// <summary>«Перевірити»: the category and who the operation is with, with the <c>Version</c> the user opened.</summary>
public sealed record FinanceTransactionReviewInput(Guid CategoryId, FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName, int Version);
