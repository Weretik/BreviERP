using Reminders.Api.Endpoints.Telegram.ConnectTelegramWebhook;
using Reminders.Api.Endpoints.Telegram.ReceiveTelegramUpdate;

namespace Reminders.Api.Endpoints.Telegram;

/// <summary>Telegram webhook (anonymous, secret header) and its connection by an admin.</summary>
internal static class TelegramEndpoints
{
    public static IEndpointRouteBuilder MapTelegramEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reminders/telegram").WithTags("TelegramWebhook");
        ReceiveTelegramUpdateEndpoint.Map(group);
        ConnectTelegramWebhookEndpoint.Map(group);
        return app;
    }
}
