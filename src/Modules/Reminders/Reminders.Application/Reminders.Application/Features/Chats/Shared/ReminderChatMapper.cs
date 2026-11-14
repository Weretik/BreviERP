using Reminders.Domain.Chats;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Chats.Shared;

public static class ReminderChatMapper
{
    public static ReminderChatResponse ToResponse(ReminderChat chat) => new(chat.Id.Value, chat.TelegramChatId,
        ReminderCodes.ChatKind(chat.Kind), chat.TelegramTitle, chat.DisplayName, chat.IsApproved, chat.IsBotMember, chat.DiscoveredAt,
        chat.ReceivesSiteRequests);
}
