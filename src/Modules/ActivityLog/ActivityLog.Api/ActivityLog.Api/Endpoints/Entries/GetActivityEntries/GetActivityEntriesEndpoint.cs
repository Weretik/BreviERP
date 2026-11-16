using ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

namespace ActivityLog.Api.Endpoints.Entries.GetActivityEntries;

internal static class GetActivityEntriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("entries", HandleAsync).WithName("getActivityEntries")
            .WithSummary("A page of «Журнал дій», newest first")
            .WithDescription("Who created, changed or deleted what and when (ACT-001). Filters combine: person, role, " +
                             "object type, action, Kyiv period (from/to days, inclusive) and words of the object title.")
            .Produces<ActivityEntriesPageDto>();

    private static async Task<IResult> HandleAsync(
        [FromServices] ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = GetActivityEntriesQuery.DefaultPageSize,
        int? actorId = null,
        string? role = null,
        string? subjectType = null,
        string? action = null,
        DateOnly? from = null,
        DateOnly? to = null,
        string? search = null) =>
        (await sender.Send(new GetActivityEntriesQuery(page, pageSize, actorId, role, subjectType, action, from, to,
            search), cancellationToken)).ToHttpResult();
}
