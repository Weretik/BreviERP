using Reminders.Domain.Calendar;

namespace Reminders.Domain.Dispatching;

public static class ReminderSnooze
{
    /// <summary>
    /// The next day a postponed reminder is sent again. Counts working days; never later than the last
    /// working day before the deadline while that day is still ahead, so a deadline is always reminded.
    /// </summary>
    public static DateOnly Until(SnoozeOption option, DateOnly today, DateOnly dueDate, WorkingCalendar calendar)
    {
        var until = option switch
        {
            SnoozeOption.Tomorrow => calendar.AddWorkingDays(today, 1),
            SnoozeOption.TwoDays => calendar.AddWorkingDays(today, 2),
            SnoozeOption.Monday => NextMonday(today, calendar),
            _ => throw new ArgumentOutOfRangeException(nameof(option))
        };
        var lastBeforeDeadline = calendar.LastWorkingDayOnOrBefore(dueDate);
        return lastBeforeDeadline > today && until > lastBeforeDeadline ? lastBeforeDeadline : until;
    }

    private static DateOnly NextMonday(DateOnly today, WorkingCalendar calendar)
    {
        var days = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        var monday = today.AddDays(days == 0 ? 7 : days);
        return calendar.IsWorkingDay(monday) ? monday : calendar.NextWorkingDayAfter(monday);
    }
}
