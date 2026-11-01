using System.Text.Json.Serialization;

namespace Reminders.Application.Telegram;

public sealed record TelegramChatMember(
    [property: JsonPropertyName("status")] string Status);
