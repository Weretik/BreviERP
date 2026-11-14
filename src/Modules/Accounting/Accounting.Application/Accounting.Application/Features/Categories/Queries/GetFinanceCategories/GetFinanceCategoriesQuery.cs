using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Queries.GetFinanceCategories;

/// <summary>«Категорії» of one area of the books (or both when <paramref name="Scope"/> is null), roots before children.</summary>
public sealed record GetFinanceCategoriesQuery(LedgerScope? Scope) : IQuery<Result<IReadOnlyList<FinanceCategoryResponse>>>;

public sealed class GetFinanceCategoriesValidator : AbstractValidator<GetFinanceCategoriesQuery>
{
    public GetFinanceCategoriesValidator() => RuleFor(x => x.Scope).IsInEnum().When(x => x.Scope is not null);
}

public sealed class GetFinanceCategoriesHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceCategoriesQuery, Result<IReadOnlyList<FinanceCategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FinanceCategoryResponse>>> Handle(GetFinanceCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .Where(x => query.Scope == null || x.Scope == query.Scope)
            .ToListAsync(cancellationToken);
        IReadOnlyList<FinanceCategoryResponse> rows = categories
            .OrderBy(x => x.ParentId is not null).ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(FinanceCategoryResponse.From).ToList();
        return Result.Success(rows);
    }
}
