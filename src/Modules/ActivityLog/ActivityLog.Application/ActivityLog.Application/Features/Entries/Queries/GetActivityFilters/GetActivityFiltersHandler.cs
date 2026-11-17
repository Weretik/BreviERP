using ActivityLog.Contracts.Activity;

namespace ActivityLog.Application.Features.Entries.Queries.GetActivityFilters;

/// <summary>
/// People (by their latest name) and object types found in the journal, by name. They are read from the small filter
/// tables that the recording handler keeps, so the answer never groups the whole journal (database rules 2.2).
/// </summary>
public sealed class GetActivityFiltersHandler(IActivityLogDbContext db)
    : IQueryHandler<GetActivityFiltersQuery, Result<ActivityFiltersDto>>
{
    public async ValueTask<Result<ActivityFiltersDto>> Handle(GetActivityFiltersQuery query,
        CancellationToken cancellationToken)
    {
        var actors = await db.Actors.AsNoTracking().Select(x => new ActivityActorDto(x.Id.Value, x.Name))
            .ToListAsync(cancellationToken);
        var types = await db.SubjectTypes.AsNoTracking().Select(x => new ActivitySubjectTypeDto(x.Key, x.Title))
            .ToListAsync(cancellationToken);
        return Result.Success(new ActivityFiltersDto(
            actors.OrderBy(x => x.Name, StringComparer.CurrentCulture).ToArray(),
            types.OrderBy(x => x.Title, StringComparer.CurrentCulture).ToArray(),
            ActivityActions.All));
    }
}
