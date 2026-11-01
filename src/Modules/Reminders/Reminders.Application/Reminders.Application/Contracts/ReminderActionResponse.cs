namespace Reminders.Application.Contracts;

public sealed record ReminderActionResponse(string Kind, ReminderActorResponse Actor, DateTimeOffset At, DateOnly? SnoozedUntil);
