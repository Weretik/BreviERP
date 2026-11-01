namespace Reminders.Infrastructure.Telegram;

public sealed class TelegramApiException(string method, string? description) : Exception($"Telegram {method} failed: {description}")
{
    public string? Description { get; } = description;
}
