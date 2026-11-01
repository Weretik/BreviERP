namespace Reminders.Domain.Dispatching;

/// <summary>Extra line in a reminder: the deadline is today, already passed, or falls on a day off.</summary>
public readonly record struct ReminderNotice(ReminderNoticeKind Kind, int OverdueDays = 0);
