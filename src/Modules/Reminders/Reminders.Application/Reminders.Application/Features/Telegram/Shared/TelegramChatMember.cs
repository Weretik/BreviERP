using System.Text.Json.Serialization;

namespace Reminders.Application.Features.Telegram.Shared;

public sealed record TelegramChatMember(
    [property: JsonPropertyName("status")] string Status);
