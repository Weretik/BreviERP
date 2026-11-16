using ActivityLog.Contracts.Activity;

namespace ActivityLog.Application.Features.Entries.Queries.GetActivityFilters;

/// <summary>
/// People (by their latest name) and object types found in the journal, by name; grouped in SQL, so the answer is as
/// small as the staff list.
/// </summary>
public sealed class GetActivityFiltersHandler(IActivityLogDbContext db)
    : IQueryHandler<GetActivityFiltersQuery, Result<ActivityFiltersDto>>
{
    public async ValueTask<Result<ActivityFiltersDto>> Handle(GetActivityFiltersQuery query,
        CancellationToken cancellationToken)
    {
        var entries = db.Entries.AsNoTracking();
        var actors = await entries.GroupBy(x => x.ActorId)
            .Select(g => new { Id = g.Key, Last = g.Max(x => x.OccurredAt) })
            .Join(entries, g => new { g.Id, g.Last }, x => new { Id = x.ActorId, Last = x.OccurredAt },
                (g, x) => new { g.Id, x.ActorName })
            .ToListAsync(cancellationToken);
        var types = await entries.GroupBy(x => x.SubjectType)
            .Select(g => new { Key = g.Key, Title = g.Max(x => x.SubjectTypeTitle) })
            .ToListAsync(cancellationToken);
        return Result.Success(new ActivityFiltersDto(
            actors.DistinctBy(x => x.Id).OrderBy(x => x.ActorName, StringComparer.CurrentCulture)
                .Select(x => new ActivityActorDto(x.Id, x.ActorName)).ToArray(),
            types.OrderBy(x => x.Title, StringComparer.CurrentCulture)
                .Select(x => new ActivitySubjectTypeDto(x.Key, x.Title!)).ToArray(),
            ActivityActions.All));
    }
}
