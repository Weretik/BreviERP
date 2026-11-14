using Reminders.Application.Features.Chats.Commands.SendReminderChatTest;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Chats.SendReminderChatTest;

internal static class SendReminderChatTestEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/test-message", HandleAsync).WithName("sendReminderChatTest")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SendReminderChatTestCommand(id), cancellationToken)).ToNoContentHttpResult();
}
