using Reminders.Application.Features.Tasks.Commands.UpdateReminderTask;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Api.Endpoints.Tasks.UpdateReminderTask;

internal static class UpdateReminderTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("updateReminderTask").Produces<ReminderTaskResponse>();

    private static async Task<IResult> HandleAsync(long id, ReminderTaskInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateReminderTaskCommand(id, input), cancellationToken)).ToHttpResult();
}
