using Reminders.Application.Contracts;

namespace Reminders.Application.Chats.UpdateChat;

/// <summary>Names a chat for Admin and approves or blocks reminders to it.</summary>
public sealed record UpdateReminderChatCommand(long Id, ReminderChatInput Input) : ICommand<Result<ReminderChatResponse>>;
