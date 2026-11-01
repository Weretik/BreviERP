using Reminders.Application.Abstractions;

namespace Reminders.Infrastructure.Telegram;

/// <summary>Configuration section Reminders:Telegram. BotToken and WebhookSecret come from Secret Manager.</summary>
public sealed class TelegramOptions : ITelegramWebhookSettings
{
    public const string Section = "Reminders:Telegram";

    public string? BotToken { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>Public HTTPS address of POST /api/reminders/telegram/webhook.</summary>
    public string? WebhookUrl { get; set; }

    public string ApiBaseUrl { get; set; } = "https://api.telegram.org";
}
