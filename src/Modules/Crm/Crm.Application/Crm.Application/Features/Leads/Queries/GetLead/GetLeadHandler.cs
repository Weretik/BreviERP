using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Calling;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetLead;

public sealed class GetLeadHandler(ILeadStore store, TimeProvider clock)
    : IQueryHandler<GetLeadQuery, Result<LeadDetailResponse>>
{
    public async ValueTask<Result<LeadDetailResponse>> Handle(GetLeadQuery query, CancellationToken cancellationToken)
    {
        var lead = await store.Leads.AsNoTracking().Include(x => x.Calls).AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (lead is null) return Result.NotFound(LeadMessages.NotFound);
        return Result.Success(await store.DetailAsync(lead, clock.GetUtcNow(), cancellationToken));
    }
}
