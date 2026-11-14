using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Api.Endpoints.Tasks.UpdateScheduledTaskSchedule;

internal static class UpdateScheduledTaskScheduleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{key}/schedule", HandleAsync).WithName("updateScheduledTaskSchedule")
            .Produces<ScheduledTaskResponse>();

    private static async Task<IResult> HandleAsync(string key, ScheduledTaskScheduleInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateScheduledTaskScheduleCommand(key, input, SchedulingAccess.Actor(user)), cancellationToken))
        .ToHttpResult();
}
