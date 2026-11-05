using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Categories;

/// <summary>New category; <c>ParentId</c> makes it a subcategory of that root.</summary>
public sealed record FinanceCategoryInput(
    string? Name,
    CategoryDirection Direction,
    LedgerScope Scope,
    Guid? ParentId,
    string? Note,
    int SortOrder);

/// <summary>Name, note and order with the <c>Version</c> the user opened.</summary>
public sealed record FinanceCategoryUpdateInput(string? Name, string? Note, int SortOrder, int Version);
