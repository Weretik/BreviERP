using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;

/// <summary>
/// «Додати категорію» (no <paramref name="ParentId"/>) or «Додати підкатегорію». A subcategory takes the direction and
/// area of its root; the given ones must match.
/// </summary>
public sealed record CreateFinanceCategoryCommand(
    string? Name,
    CategoryDirection Direction,
    LedgerScope Scope,
    Guid? ParentId,
    string? Note,
    int SortOrder,
    string By) : ICommand<Result<FinanceCategoryResponse>>;
