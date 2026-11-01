using Reminders.Domain.Calendar;
using Reminders.Domain.Schedules;

namespace Reminders.Domain.Tasks;

/// <summary>Periodicity, deadline rule, and how many calendar days before the deadline reminders start.</summary>
public sealed record ReminderSchedule(ReminderPeriodicity Periodicity, DueRule Due, int LeadDays)
{
    public const int MaxLeadDays = 60;

    private ReminderSchedule() : this(ReminderPeriodicity.Monthly, DueRule.DayOfMonth(1, 0), 0) { }

    public DateOnly DueDate(ReminderPeriod period, WorkingCalendar calendar) => Due.DueDate(period, calendar);

    public DateOnly WindowStart(DateOnly dueDate) => dueDate.AddDays(-LeadDays);

    /// <summary>Periods whose deadline falls inside [from, to], in deadline order.</summary>
    public IEnumerable<(ReminderPeriod Period, DateOnly DueDate)> PeriodsDueBetween(
        DateOnly from, DateOnly to, WorkingCalendar calendar)
    {
        // A deadline is never more than MaxMonthOffset months plus a year after its period starts.
        var period = ReminderPeriod.Containing(Periodicity, from.AddMonths(-(DueRule.MaxMonthOffset + 13)));
        while (period.Start <= to)
        {
            var due = DueDate(period, calendar);
            if (due >= from && due <= to) yield return (period, due);
            period = period.Next();
        }
    }
}
