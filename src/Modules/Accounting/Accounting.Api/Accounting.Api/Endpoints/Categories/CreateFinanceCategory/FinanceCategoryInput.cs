using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Endpoints.Categories.CreateFinanceCategory;

/// <summary>New category; <c>ParentId</c> makes it a subcategory of that root.</summary>
public sealed record FinanceCategoryInput(
    string? Name,
    CategoryDirection Direction,
    LedgerScope Scope,
    Guid? ParentId,
    string? Note,
    int SortOrder);
