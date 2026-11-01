namespace Reminders.Api.Tasks;

public sealed record AssignReminderChatRequest(long ChatId, IReadOnlyList<string> Organizations);
