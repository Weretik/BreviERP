using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.RunScheduledTaskNow;

internal static class RunScheduledTaskNowEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{key}/run", HandleAsync).WithName("runScheduledTaskNow")
            .Produces<ScheduledTaskResponse>();

    private static async Task<IResult> HandleAsync(string key, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RunScheduledTaskNowCommand(key, SchedulingAccess.Actor(user)), cancellationToken)).ToHttpResult();
}
