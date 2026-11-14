namespace Reminders.Application.Features.Occurrences.Shared;

public sealed record ReminderActionResponse(string Kind, ReminderActorResponse Actor, DateTimeOffset At, DateOnly? SnoozedUntil);
