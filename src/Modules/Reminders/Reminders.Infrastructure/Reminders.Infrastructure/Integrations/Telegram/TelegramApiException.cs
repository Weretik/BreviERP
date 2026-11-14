using Reminders.Application.Abstractions;

namespace Reminders.Infrastructure.Integrations.Telegram;

public sealed class TelegramApiException(string method, string? description) : TelegramGatewayException($"Telegram {method} failed: {description}")
{
    public string? Description { get; } = description;
}
