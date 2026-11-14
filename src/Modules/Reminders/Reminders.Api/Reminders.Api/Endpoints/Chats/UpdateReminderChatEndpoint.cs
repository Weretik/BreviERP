using Reminders.Application.Features.Chats.Commands.UpdateReminderChat;
using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Api.Endpoints.Chats;

internal static class UpdateReminderChatEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("updateReminderChat").Produces<ReminderChatResponse>();

    private static async Task<IResult> HandleAsync(long id, ReminderChatInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateReminderChatCommand(id, input), cancellationToken)).ToHttpResult();
}
