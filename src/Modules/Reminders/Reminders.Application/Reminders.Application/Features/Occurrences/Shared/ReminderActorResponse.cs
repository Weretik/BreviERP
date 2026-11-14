namespace Reminders.Application.Features.Occurrences.Shared;

public sealed record ReminderActorResponse(string Name, long? TelegramUserId, int? AdminUserId);
