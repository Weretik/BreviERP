using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Application.Features.Chats.Queries.GetReminderChats;

public sealed record GetReminderChatsQuery : IQuery<Result<IReadOnlyList<ReminderChatResponse>>>;
