using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Queries.GetFinanceCategories;

/// <summary>«Категорії» of one area of the books (or both when <paramref name="Scope"/> is null), roots before children.</summary>
public sealed record GetFinanceCategoriesQuery(LedgerScope? Scope) : IQuery<Result<IReadOnlyList<FinanceCategoryResponse>>>;
