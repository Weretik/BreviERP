using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Application.Contracts;

/// <summary>A category of «Категорії»; <c>ParentId</c> is null for a root.</summary>
public sealed record FinanceCategoryResponse(
    Guid Id,
    string Name,
    CategoryDirection Direction,
    LedgerScope Scope,
    Guid? ParentId,
    string Note,
    bool Active,
    int SortOrder,
    int Version)
{
    public static FinanceCategoryResponse From(FinanceCategory x) => new(x.Id.Value, x.Name, x.Direction, x.Scope,
        x.ParentId?.Value, x.Note, x.Active, x.SortOrder, x.Version);
}
