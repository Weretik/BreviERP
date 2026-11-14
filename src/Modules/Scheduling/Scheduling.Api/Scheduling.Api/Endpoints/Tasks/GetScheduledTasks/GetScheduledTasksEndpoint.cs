using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;

internal static class GetScheduledTasksEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getScheduledTasks")
            .Produces<IReadOnlyList<ScheduledTaskResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTasksQuery(), cancellationToken)).ToHttpResult();
}
