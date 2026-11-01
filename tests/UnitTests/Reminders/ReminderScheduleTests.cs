using Reminders.Domain.Calendar;
using Reminders.Domain.Schedules;
using Reminders.Infrastructure.Persistence.Seeding;

namespace UnitTests.Reminders;

public sealed class ReminderScheduleTests
{
    private static readonly WorkingCalendar Calendar = WorkingCalendar.WeekendsOnly;

    // Every default task of the brief, with a real period and the deadline the accountant expects.
    [TestCase(0, "2026-10-01", "2026-11-05")]
    [TestCase(1, "2026-10-01", "2026-11-18")]
    [TestCase(2, "2026-10-01", "2026-11-20")]
    [TestCase(3, "2026-10-01", "2026-11-20")]
    [TestCase(4, "2026-10-01", "2026-11-30")]
    [TestCase(4, "2027-01-01", "2027-02-28")]
    [TestCase(5, "2026-07-01", "2026-11-09")]
    [TestCase(6, "2026-01-01", "2027-02-28")]
    [TestCase(7, "2026-01-01", "2027-02-09")]
    [TestCase(8, "2026-11-01", "2026-11-15")]
    [TestCase(9, "2026-10-01", "2026-10-30")]
    [TestCase(10, "2026-07-01", "2026-10-20")]
    [TestCase(11, "2026-10-01", "2026-11-20")]
    [TestCase(12, "2026-07-01", "2026-11-09")]
    [TestCase(13, "2026-07-01", "2026-11-09")]
    [TestCase(14, "2026-07-01", "2026-11-19")]
    [TestCase(15, "2026-07-01", "2026-11-19")]
    [TestCase(16, "2026-07-01", "2026-11-09")]
    [TestCase(17, "2026-11-01", "2026-11-15")]
    [TestCase(18, "2026-11-01", "2026-11-30")]
    [TestCase(19, "2026-10-01", "2026-11-18")]
    [TestCase(20, "2026-10-01", "2026-11-20")]
    [TestCase(21, "2026-11-01", "2026-11-12")]
    [TestCase(22, "2026-11-01", "2026-11-15")]
    [TestCase(23, "2026-10-01", "2026-11-01")]
    [TestCase(24, "2026-10-01", "2026-11-05")]
    public void Default_task_deadline_matches_the_brief(int index, string periodStart, string expected)
    {
        var (_, _, schedule) = DefaultReminderTasks.All[index];
        var period = new ReminderPeriod(schedule.Periodicity, DateOnly.Parse(periodStart));
        Assert.That(schedule.DueDate(period, Calendar), Is.EqualTo(DateOnly.Parse(expected)));
    }

    [Test]
    public void Default_list_has_25_tasks_with_five_day_lead() =>
        Assert.That(DefaultReminderTasks.All.Select(x => x.Schedule.LeadDays).ToArray(), Has.Length.EqualTo(25).And.All.EqualTo(5));

    [Test]
    public void Last_working_day_skips_extra_non_working_days()
    {
        var calendar = new WorkingCalendar([new DateOnly(2026, 10, 30)]);
        var due = DueRule.LastWorkingDay(0).DueDate(new ReminderPeriod(ReminderPeriodicity.Monthly, new DateOnly(2026, 10, 1)), calendar);
        Assert.That(due, Is.EqualTo(new DateOnly(2026, 10, 29)));
    }

    [Test]
    public void Periods_due_between_returns_each_deadline_once_in_order()
    {
        var (_, _, schedule) = DefaultReminderTasks.All[2];
        var periods = schedule.PeriodsDueBetween(new DateOnly(2026, 11, 1), new DateOnly(2027, 1, 31), Calendar).ToArray();
        Assert.That(periods.Select(x => x.DueDate), Is.EqualTo(new[]
            { new DateOnly(2026, 11, 20), new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 20) }));
        Assert.That(periods[0].Period.Label, Is.EqualTo("жовтень 2026"));
    }

    [Test]
    public void Period_labels_are_ukrainian()
    {
        Assert.That(new ReminderPeriod(ReminderPeriodicity.Quarterly, new DateOnly(2026, 7, 1)).Label, Is.EqualTo("III квартал 2026"));
        Assert.That(new ReminderPeriod(ReminderPeriodicity.Yearly, new DateOnly(2026, 1, 1)).Label, Is.EqualTo("2026 рік"));
    }

    [TestCase(DueRuleKind.DayOfMonth, 0, 0, false)]
    [TestCase(DueRuleKind.DayOfMonth, 31, 12, true)]
    [TestCase(DueRuleKind.DayOfMonth, 5, 13, false)]
    [TestCase(DueRuleKind.DaysAfterPeriodEnd, 40, 0, true)]
    [TestCase(DueRuleKind.DaysAfterPeriodEnd, 0, 0, false)]
    [TestCase(DueRuleKind.LastWorkingDayOfMonth, 0, 1, true)]
    public void Rule_shape_is_validated(DueRuleKind kind, int day, int offset, bool valid) =>
        Assert.That(DueRule.Validate(kind, day, offset), valid ? Is.Empty : Is.Not.Empty);
}
