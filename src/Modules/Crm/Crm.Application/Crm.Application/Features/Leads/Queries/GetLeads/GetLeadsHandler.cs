using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetLeads;

public sealed class GetLeadsHandler(ILeadStore store, TimeProvider clock)
    : IQueryHandler<GetLeadsQuery, Result<LeadPageResponse>>
{
    public async ValueTask<Result<LeadPageResponse>> Handle(GetLeadsQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var leads = store.Leads.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
            var number = int.TryParse(query.Search.Trim().TrimStart('№', ' '), out var n) ? n : 0;
            leads = leads.Where(x => x.SourceId == number
                                     || EF.Functions.ILike(x.Company, term)
                                     || (x.Region != null && EF.Functions.ILike(x.Region, term))
                                     || (x.Industry != null && EF.Functions.ILike(x.Industry, term))
                                     || (x.Contacts != null && EF.Functions.ILike(x.Contacts, term))
                                     || (x.OwnerName != null && EF.Functions.ILike(x.OwnerName, term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Category)) leads = leads.Where(x => x.Category == query.Category);
        if (query.Stage == "Open")
            leads = leads.Where(x => x.Stage != LeadStage.Client && x.Stage != LeadStage.Closed);
        else if (Enum.TryParse<LeadStage>(query.Stage, out var stage)) leads = leads.Where(x => x.Stage == stage);

        var userId = query.Actor.UserId;
        var counts = await leads.GroupBy(_ => 1).Select(g => new
        {
            All = g.Count(),
            Mine = g.Count(x => x.OwnerUserId == userId),
            Free = g.Count(x => x.OwnerUserId == null && x.Stage != LeadStage.Client && x.Stage != LeadStage.Closed),
        }).SingleOrDefaultAsync(cancellationToken);

        leads = query.Scope switch
        {
            "Mine" => leads.Where(x => x.OwnerUserId == userId),
            "Free" => leads.Where(x => x.OwnerUserId == null && x.Stage != LeadStage.Client && x.Stage != LeadStage.Closed),
            _ => leads,
        };

        var total = await leads.CountAsync(cancellationToken);
        var items = await leads
            .OrderByDescending(x => x.InSheet)
            .ThenByDescending(x => x.AddedOn)
            .ThenByDescending(x => x.SourceId)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToSummaries(now)
            .ToListAsync(cancellationToken);

        return Result.Success(new LeadPageResponse(items, query.Page, query.PageSize, total, counts?.Mine ?? 0,
            counts?.Free ?? 0, counts?.All ?? 0));
    }
}
