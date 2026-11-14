namespace Reminders.Application.Features.Occurrences.Shared;

public sealed record ReminderOccurrenceResponse(
    long Id,
    long TaskId,
    string TaskTitle,
    string Organization,
    string Period,
    DateOnly DueDate,
    DateOnly RemindFrom,
    string Status,
    DateOnly? SnoozedUntil,
    ReminderActorResponse? ClosedBy,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<ReminderActionResponse> Actions);
