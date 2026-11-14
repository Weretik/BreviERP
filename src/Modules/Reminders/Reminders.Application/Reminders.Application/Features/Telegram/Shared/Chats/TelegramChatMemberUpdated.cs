using System.Text.Json.Serialization;

namespace Reminders.Application.Features.Telegram.Shared.Chats;

public sealed record TelegramChatMemberUpdated(
    [property: JsonPropertyName("chat")] TelegramChat Chat,
    [property: JsonPropertyName("new_chat_member")] TelegramChatMember NewChatMember);
