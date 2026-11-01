using Reminders.Domain.Calendar;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;

namespace Reminders.Domain.Dispatching;

public static class ReminderPlanner
{
    /// <summary>Reminders go out on working days from the window start until the occurrence is done.</summary>
    public static bool ShouldRemind(ReminderTask task, ReminderOccurrence occurrence, DateOnly today, WorkingCalendar calendar) =>
        task.CanSend
        && occurrence.Status == ReminderOccurrenceStatus.Open
        && calendar.IsWorkingDay(today)
        && today >= task.Schedule.WindowStart(occurrence.DueDate)
        && !occurrence.IsPostponedOn(today);

    public static ReminderNotice Notice(DateOnly dueDate, DateOnly today, WorkingCalendar calendar)
    {
        if (today > dueDate) return new ReminderNotice(ReminderNoticeKind.Overdue, today.DayNumber - dueDate.DayNumber);
        if (today == dueDate) return new ReminderNotice(ReminderNoticeKind.DueToday);
        if (!calendar.IsWorkingDay(dueDate) && calendar.LastWorkingDayOnOrBefore(dueDate) == today)
            return new ReminderNotice(ReminderNoticeKind.DueOnNonWorkingDay);
        return new ReminderNotice(ReminderNoticeKind.None);
    }
}
