using Reminders.Application.Features.Tasks.Queries.GetReminderTasks;
using Reminders.Application.Features.Tasks.Shared;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Tasks.GetReminderTasks;

internal static class GetReminderTasksEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getReminderTasks").Produces<IReadOnlyList<ReminderTaskResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetReminderTasksQuery(), cancellationToken)).ToHttpResult();
}
