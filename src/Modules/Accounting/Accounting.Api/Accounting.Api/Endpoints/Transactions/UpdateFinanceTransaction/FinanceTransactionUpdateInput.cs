using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Endpoints.Transactions.UpdateFinanceTransaction;

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
