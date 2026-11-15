using Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.GetScheduledTaskRuns;

internal static class GetScheduledTaskRunsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{key}/runs", HandleAsync).WithName("getScheduledTaskRuns")
            .WithSummary("The last 50 runs of a task, newest first")
            .WithDescription("The last 50 runs of a task, newest first. Runs are kept 90 days.")
            .Produces<IReadOnlyList<ScheduledTaskRunResponse>>();

    private static async Task<IResult> HandleAsync(string key, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetScheduledTaskRunsQuery(key), cancellationToken)).ToHttpResult();
}
