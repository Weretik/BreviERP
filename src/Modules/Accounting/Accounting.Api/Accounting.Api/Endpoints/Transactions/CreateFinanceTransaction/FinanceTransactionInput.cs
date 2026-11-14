using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Endpoints.Transactions.CreateFinanceTransaction;

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
