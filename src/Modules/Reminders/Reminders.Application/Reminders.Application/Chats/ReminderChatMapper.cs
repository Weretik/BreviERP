using Reminders.Application.Contracts;
using Reminders.Domain.Chats;

namespace Reminders.Application.Chats;

public static class ReminderChatMapper
{
    public static ReminderChatResponse ToResponse(ReminderChat chat) => new(chat.Id.Value, chat.TelegramChatId,
        ReminderCodes.ChatKind(chat.Kind), chat.TelegramTitle, chat.DisplayName, chat.IsApproved, chat.IsBotMember, chat.DiscoveredAt);
}
