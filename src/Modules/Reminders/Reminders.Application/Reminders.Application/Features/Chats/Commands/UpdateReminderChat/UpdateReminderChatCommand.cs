using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Application.Features.Chats.Commands.UpdateReminderChat;

/// <summary>Names a chat for Admin and approves or blocks reminders to it.</summary>
public sealed record UpdateReminderChatCommand(long Id, ReminderChatInput Input) : ICommand<Result<ReminderChatResponse>>;
