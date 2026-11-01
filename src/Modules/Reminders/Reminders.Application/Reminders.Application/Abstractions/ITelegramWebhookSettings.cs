namespace Reminders.Application.Abstractions;

/// <summary>Public webhook address and the secret Telegram repeats in X-Telegram-Bot-Api-Secret-Token.</summary>
public interface ITelegramWebhookSettings
{
    string? WebhookUrl { get; }
    string? WebhookSecret { get; }
}
