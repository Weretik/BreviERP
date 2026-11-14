using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Queries.GetLeadStats;

public sealed class GetLeadStatsHandler(ILeadStore store, TimeProvider clock)
    : IQueryHandler<GetLeadStatsQuery, Result<LeadStatsResponse>>
{
    private const int IndustryLimit = 20;

    public async ValueTask<Result<LeadStatsResponse>> Handle(GetLeadStatsQuery query,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var today = LeadClock.KyivDate(now);
        var to = query.To ?? today;
        var from = query.From ?? to.AddDays(-6);
        var start = LeadClock.StartOf(from);
        var end = LeadClock.StartOf(to.AddDays(1));

        var leads = await store.Leads.AsNoTracking()
            .Select(x => new { x.Stage, x.OwnerUserId, x.OwnerName, x.NextContactAt, x.Category, x.Industry, x.CreatedAt })
            .ToListAsync(cancellationToken);
        var calls = await store.LeadCalls.AsNoTracking()
            .Where(x => x.At >= start && x.At < end)
            .Select(x => new { x.ByUserId, x.ByName, x.ResultName, x.Outcome })
            .ToListAsync(cancellationToken);
        var sync = await store.LeadSyncStates.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == LeadSyncState.GoogleSheet, cancellationToken);

        bool Open(LeadStage stage) => stage is not (LeadStage.Client or LeadStage.Closed);
        var open = leads.Where(x => Open(x.Stage)).ToList();

        var people = open.Where(x => x.OwnerUserId is not null)
            .Select(x => (Id: x.OwnerUserId!.Value, Name: x.OwnerName!))
            .Concat(calls.Select(x => (Id: x.ByUserId, Name: x.ByName)))
            .GroupBy(x => x.Id)
            .Select(g => new LeadPersonResponse(g.Key, g.Last().Name));

        var managers = people.Select(person =>
            {
                var own = open.Where(x => x.OwnerUserId == person.UserId).ToList();
                var made = calls.Where(x => x.ByUserId == person.UserId).ToList();
                return new LeadManagerStatsResponse(
                    person,
                    own.Count,
                    made.Count,
                    made.Count == 0 ? null : (int)Math.Round(100.0 * made.Count(x => x.Outcome != LeadOutcome.Retry) / made.Count),
                    made.Count(x => x.Outcome == LeadOutcome.Offer),
                    made.Count(x => x.Outcome == LeadOutcome.Client),
                    own.Count(x => x.NextContactAt < now));
            })
            .OrderByDescending(x => x.Calls).ThenBy(x => x.Manager.Name, StringComparer.CurrentCulture)
            .ToArray();

        var results = calls.GroupBy(x => x.ResultName)
            .Select(g => new LeadCountResponse(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).ThenBy(x => x.Name, StringComparer.CurrentCulture)
            .ToArray();

        var categories = leads.Select(x => x.Category ?? "—").Distinct().Order(StringComparer.Ordinal).ToArray();
        var industries = leads.GroupBy(x => x.Industry ?? "Не вказано")
            .Select(g => new LeadIndustryStatsResponse(
                g.Key,
                g.GroupBy(x => x.Category ?? "—").Select(c => new LeadCountResponse(c.Key, c.Count()))
                    .OrderBy(x => x.Name, StringComparer.Ordinal).ToArray(),
                g.Count()))
            .OrderByDescending(x => x.Total).ThenBy(x => x.Industry, StringComparer.CurrentCulture)
            .Take(IndustryLimit)
            .ToArray();

        return Result.Success(new LeadStatsResponse(
            from,
            to,
            leads.Count,
            open.Count(x => x.OwnerUserId is null),
            open.Count(x => x.OwnerUserId is not null && x.Stage is LeadStage.InWork or LeadStage.Talking),
            open.Count(x => x.Stage == LeadStage.Offer),
            leads.Count(x => x.Stage == LeadStage.Client),
            leads.Count(x => x.Stage == LeadStage.Closed),
            open.Count(x => x.OwnerUserId is not null && x.NextContactAt < now),
            calls.Count,
            leads.Count(x => x.CreatedAt >= start && x.CreatedAt < end),
            managers,
            results,
            industries,
            categories,
            sync is null
                ? null
                : new LeadSyncStateResponse(sync.LastRunAt, sync.LastSuccessAt, sync.Rows, sync.Added, sync.Updated,
                    sync.Error, sync.Warning)));
    }
}
