using System.Text.Json.Serialization;

namespace Reminders.Application.Features.Telegram.Shared;

public sealed record TelegramMessage(
    [property: JsonPropertyName("message_id")] long MessageId,
    [property: JsonPropertyName("chat")] TelegramChat Chat,
    [property: JsonPropertyName("from")] TelegramUser? From,
    [property: JsonPropertyName("text")] string? Text);
