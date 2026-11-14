using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Queries.GetFinanceCategories;

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
