using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetMyDay;

public sealed class GetMyDayHandler(ILeadStore store, TimeProvider clock)
    : IQueryHandler<GetMyDayQuery, Result<LeadMyDayResponse>>
{
    private const int LaterLimit = 50;

    public async ValueTask<Result<LeadMyDayResponse>> Handle(GetMyDayQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var startOfDay = LeadClock.StartOfToday(now);
        var endOfDay = LeadClock.EndOfToday(now);
        var userId = query.Actor.UserId;

        var mine = store.Leads.AsNoTracking()
            .Where(x => x.OwnerUserId == userId && x.Stage != LeadStage.Client && x.Stage != LeadStage.Closed);

        var queue = await mine
            .Where(x => x.NextContactAt == null || x.NextContactAt < endOfDay)
            .OrderBy(x => x.NextContactAt == null)
            .ThenBy(x => x.NextContactAt)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.TakenAt)
            .ToSummaries(now)
            .ToListAsync(cancellationToken);

        var later = await mine
            .Where(x => x.NextContactAt >= endOfDay)
            .OrderBy(x => x.NextContactAt)
            .Take(LaterLimit)
            .ToSummaries(now)
            .ToListAsync(cancellationToken);

        var callsToday = await store.LeadCalls.AsNoTracking()
            .CountAsync(x => x.ByUserId == userId && x.At >= startOfDay, cancellationToken);
        var free = await store.Leads.AsNoTracking()
            .CountAsync(x => x.OwnerUserId == null && x.Stage != LeadStage.Client && x.Stage != LeadStage.Closed,
                cancellationToken);

        return Result.Success(new LeadMyDayResponse(queue, later, callsToday, callsToday + queue.Count, free));
    }
}
