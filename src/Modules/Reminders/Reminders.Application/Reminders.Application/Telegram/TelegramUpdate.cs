using System.Text.Json.Serialization;

namespace Reminders.Application.Telegram;

public sealed record TelegramUpdate(
    [property: JsonPropertyName("update_id")] long UpdateId,
    [property: JsonPropertyName("message")] TelegramMessage? Message,
    [property: JsonPropertyName("callback_query")] TelegramCallbackQuery? CallbackQuery,
    [property: JsonPropertyName("my_chat_member")] TelegramChatMemberUpdated? MyChatMember);
