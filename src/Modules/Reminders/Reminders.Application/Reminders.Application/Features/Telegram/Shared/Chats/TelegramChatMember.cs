using System.Text.Json.Serialization;

namespace Reminders.Application.Features.Telegram.Shared.Chats;

public sealed record TelegramChatMember(
    [property: JsonPropertyName("status")] string Status);
