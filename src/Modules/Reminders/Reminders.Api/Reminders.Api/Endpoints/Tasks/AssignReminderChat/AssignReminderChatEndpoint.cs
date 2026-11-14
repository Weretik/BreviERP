using Reminders.Application.Features.Tasks.Commands.AssignReminderChat;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Tasks.AssignReminderChat;

internal static class AssignReminderChatEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("assign-chat", HandleAsync).WithName("assignReminderChat").Produces<int>()
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(AssignReminderChatRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new AssignReminderChatCommand(request.ChatId, request.Organizations), cancellationToken)).ToHttpResult();
}

public sealed record AssignReminderChatRequest(long ChatId, IReadOnlyList<string> Organizations);
