using Accounting.Application.Lookup;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Contracts;

/// <summary>
/// A row of «Операції». <c>AmountMinor</c> is positive kopecks; <c>Type</c> gives the sign. <c>CategoryId</c> is the
/// chosen category; <c>CategoryRootId</c>/<c>CategoryRootName</c> is its root (the same for a root) and
/// <c>SubcategoryName</c> is set only when a subcategory was chosen.
/// </summary>
public sealed record FinanceTransactionResponse(
    Guid Id,
    DateOnly Date,
    CategoryDirection Type,
    Guid AccountId,
    string AccountName,
    long AmountMinor,
    string Currency,
    Guid? CategoryId,
    Guid? CategoryRootId,
    string? CategoryRootName,
    string? SubcategoryName,
    string OneoffName,
    string Description,
    string Comment,
    MovementSource Source,
    VerificationStatus Verification,
    LedgerScope Scope,
    MovementStatus Status,
    int Version,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    internal static FinanceTransactionResponse From(FinanceTransaction x, FinanceNames names)
    {
        var category = names.Category(x.CategoryId);
        var root = names.Root(category);
        return new FinanceTransactionResponse(x.Id.Value, x.Date, x.Type, x.AccountId.Value, names.Account(x.AccountId),
            x.AmountMinor, x.Currency, x.CategoryId?.Value, root?.Id.Value, root?.Name,
            category is { IsRoot: false } ? category.Name : null, x.OneoffName, x.Description, x.Comment, x.Source,
            x.Verification, x.Scope, x.Status, x.Version, x.CreatedBy, x.CreatedAt, x.UpdatedAt);
    }
}
