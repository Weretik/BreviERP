namespace Reminders.Application.Dispatch;

public sealed record ReminderDispatchSummary(DateOnly Date, bool WorkingDay, int CreatedOccurrences, int Sent, int Failed);
