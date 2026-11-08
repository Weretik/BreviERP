using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Persistence;

namespace Crm.Application.Leads.References;

/// <summary>«Довідники › Категорії» (R-11) with the number of leads in each.</summary>
public sealed record GetLeadCategoriesQuery : IQuery<Result<IReadOnlyList<LeadCategoryResponse>>>;

public sealed class GetLeadCategoriesQueryHandler(ILeadStore store)
    : IQueryHandler<GetLeadCategoriesQuery, Result<IReadOnlyList<LeadCategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<LeadCategoryResponse>>> Handle(GetLeadCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var counts = await store.Leads.AsNoTracking().Where(x => x.Category != null)
            .GroupBy(x => x.Category!).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var categories = await store.LeadCategories.AsNoTracking().OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<LeadCategoryResponse>>(categories
            .Select(x => new LeadCategoryResponse(x.Code, x.Description, counts.GetValueOrDefault(x.Code)))
            .ToArray());
    }
}
