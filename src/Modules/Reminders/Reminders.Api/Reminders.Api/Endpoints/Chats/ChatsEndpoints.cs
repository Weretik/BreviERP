namespace Reminders.Api.Endpoints.Chats;

/// <summary>Telegram chats the bot sees: approve, rename, test message.</summary>
internal static class ChatsEndpoints
{
    public static IEndpointRouteBuilder MapChatsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reminders/chats").RequireAuthorization(RemindersAccess.Policy).WithTags("ReminderChats");
        GetReminderChatsEndpoint.Map(group);
        UpdateReminderChatEndpoint.Map(group);
        SendReminderChatTestEndpoint.Map(group);
        return app;
    }
}
