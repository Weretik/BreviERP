namespace Reminders.Application.Contracts;

public sealed record ReminderActorResponse(string Name, long? TelegramUserId, int? AdminUserId);
