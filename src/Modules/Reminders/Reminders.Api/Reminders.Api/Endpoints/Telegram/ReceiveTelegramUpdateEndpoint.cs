using System.Security.Cryptography;
using System.Text;
using Reminders.Application.Abstractions;
using Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;
using Reminders.Application.Features.Telegram.Shared;

namespace Reminders.Api.Endpoints.Telegram;

/// <summary>Called by Telegram only. The shared secret header is the authentication.</summary>
internal static class ReceiveTelegramUpdateEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("webhook", HandleAsync).WithName("receiveTelegramUpdate")
            .AllowAnonymous().ExcludeFromDescription();

    public const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    private static async Task<IResult> HandleAsync([FromBody] TelegramUpdate update, HttpRequest request,
        [FromServices] ITelegramWebhookSettings settings, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (!SecretMatches(request.Headers[SecretHeader].ToString(), settings.WebhookSecret)) return Results.Unauthorized();
        await sender.Send(new HandleTelegramUpdateCommand(update), cancellationToken);
        return Results.Ok();
    }

    private static bool SecretMatches(string received, string? expected)
    {
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(received)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(received), Encoding.UTF8.GetBytes(expected));
    }
}
