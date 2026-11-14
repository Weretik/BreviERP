using Reminders.Application.Features.Tasks.Commands.CreateReminderTask;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Api.Endpoints.Tasks.CreateReminderTask;

internal static class CreateReminderTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createReminderTask").Produces<ReminderTaskResponse>();

    private static async Task<IResult> HandleAsync(ReminderTaskInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateReminderTaskCommand(input), cancellationToken)).ToHttpResult();
}
