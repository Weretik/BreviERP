namespace Reminders.Domain.Dispatching;

public enum ReminderNoticeKind
{
    None = 0,
    DueOnNonWorkingDay = 1,
    DueToday = 2,
    Overdue = 3
}
