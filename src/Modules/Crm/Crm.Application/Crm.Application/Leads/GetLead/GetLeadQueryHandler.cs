using Crm.Application.Leads.Access;
using Crm.Application.Leads.Calling;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Persistence;

namespace Crm.Application.Leads.GetLead;

public sealed class GetLeadQueryHandler(ILeadStore store, TimeProvider clock)
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
