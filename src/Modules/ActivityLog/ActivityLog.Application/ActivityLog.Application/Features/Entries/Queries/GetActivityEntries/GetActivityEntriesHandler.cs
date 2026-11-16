namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

/// <summary>
/// A page of «Журнал дій», newest first, with every filter combined (ACT-001 R-09). Read without tracking and paged in
/// SQL; the search is one escaped <c>LIKE</c> per word on the indexed <c>SearchText</c>.
/// </summary>
public sealed class GetActivityEntriesHandler(IActivityLogDbContext db)
    : IQueryHandler<GetActivityEntriesQuery, Result<ActivityEntriesPageDto>>
{
    private static readonly TimeZoneInfo Kyiv = FindKyiv();

    public async ValueTask<Result<ActivityEntriesPageDto>> Handle(GetActivityEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var rows = db.Entries.AsNoTracking();
        if (query.ActorId is { } actorId) rows = rows.Where(x => x.ActorId == actorId);
        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var token = ActivityEntry.RoleToken(query.Role);
            rows = rows.Where(x => x.ActorRoles.Contains(token));
        }

        if (!string.IsNullOrWhiteSpace(query.SubjectType)) rows = rows.Where(x => x.SubjectType == query.SubjectType);
        if (!string.IsNullOrWhiteSpace(query.Action)) rows = rows.Where(x => x.Action == query.Action);
        if (query.From is { } from)
        {
            var start = StartOfKyivDay(from);
            rows = rows.Where(x => x.OccurredAt >= start);
        }

        if (query.To is { } to)
        {
            var end = StartOfKyivDay(to.AddDays(1));
            rows = rows.Where(x => x.OccurredAt < end);
        }

        rows = rows.WhereSearchText(query.Search);

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = page.Select(x => new ActivityEntryDto(x.Id, x.OccurredAt, x.ActorId, x.ActorName, x.RoleNames,
                x.Action, x.SubjectType, x.SubjectTypeTitle, x.SubjectId, x.SubjectTitle, x.Summary,
                x.Changes.Select(c => new ActivityChangeDto(c.Field, c.From, c.To)).ToArray()))
            .ToArray();
        return Result.Success(new ActivityEntriesPageDto(items, query.Page, query.PageSize, total));
    }

    private static DateTimeOffset StartOfKyivDay(DateOnly day)
    {
        var local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Kyiv.GetUtcOffset(local)).ToUniversalTime();
    }

    private static TimeZoneInfo FindKyiv()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
        }
    }
}
