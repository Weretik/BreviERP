using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetLeadCategories;

public sealed class GetLeadCategoriesHandler(ILeadStore store)
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
