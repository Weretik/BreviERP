using Reminders.Application.Features.Occurrences.Queries.GetReminderOccurrences;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Api.Endpoints.Occurrences.GetReminderOccurrences;

internal static class GetReminderOccurrencesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getReminderOccurrences").Produces<ReminderOccurrencePage>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? status = null, long? taskId = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetReminderOccurrencesQuery(status, taskId, page, pageSize), cancellationToken)).ToHttpResult();
}
