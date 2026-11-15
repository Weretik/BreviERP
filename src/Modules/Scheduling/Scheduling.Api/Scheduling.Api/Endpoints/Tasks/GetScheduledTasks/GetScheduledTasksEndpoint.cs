using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;

internal static class GetScheduledTasksEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getScheduledTasks")
            .WithSummary("Every task in page order with its schedule, next run and last success and failure")
            .Produces<IReadOnlyList<ScheduledTaskResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTasksQuery(), cancellationToken)).ToHttpResult();
}
