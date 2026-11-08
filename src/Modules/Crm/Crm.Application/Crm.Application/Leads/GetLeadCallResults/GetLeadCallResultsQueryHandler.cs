using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Mapping;
using Crm.Application.Leads.Persistence;

namespace Crm.Application.Leads.GetLeadCallResults;

public sealed class GetLeadCallResultsQueryHandler(ILeadStore store)
    : IQueryHandler<GetLeadCallResultsQuery, Result<IReadOnlyList<LeadCallResultResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<LeadCallResultResponse>>> Handle(GetLeadCallResultsQuery query,
        CancellationToken cancellationToken)
    {
        var results = store.LeadCallResults.AsNoTracking();
        if (query.ActiveOnly) results = results.Where(x => x.IsActive);
        var list = await results.OrderBy(x => x.Position).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<LeadCallResultResponse>>(list.Select(x => x.ToResponse()).ToArray());
    }
}
