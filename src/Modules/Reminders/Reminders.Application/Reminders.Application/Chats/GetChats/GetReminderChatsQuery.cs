using Reminders.Application.Contracts;

namespace Reminders.Application.Chats.GetChats;

public sealed record GetReminderChatsQuery : IQuery<Result<IReadOnlyList<ReminderChatResponse>>>;
