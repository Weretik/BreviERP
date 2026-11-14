namespace Reminders.Application.Features.Dispatch.Shared;

public sealed record ReminderDispatchSummary(
    DateOnly Date,
    bool WorkingDay,
    int CreatedOccurrences,
    int Sent,
    int Failed,
    int ActiveTasks = 0,
    int TasksWithoutChat = 0,
    int NotDueYet = 0,
    int AlreadySent = 0,
    int ChatUnavailable = 0);
