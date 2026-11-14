using Reminders.Application.Features.Chats.Queries.GetReminderChats;
using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Api.Endpoints.Chats;

internal static class GetReminderChatsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getReminderChats").Produces<IReadOnlyList<ReminderChatResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetReminderChatsQuery(), cancellationToken)).ToHttpResult();
}
