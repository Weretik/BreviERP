using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetLeadCallResults;

public sealed class GetLeadCallResultsHandler(ILeadStore store)
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
