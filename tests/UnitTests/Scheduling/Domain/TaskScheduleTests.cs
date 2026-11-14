using System.Globalization;
using Scheduling.Domain.Schedules;

namespace UnitTests.Scheduling.Domain;

public sealed class TaskScheduleTests
{
    private static DateTimeOffset Kyiv(string local) => KyivTime.ToInstant(DateTime.Parse(local, CultureInfo.InvariantCulture));

    [TestCase("2026-11-16 06:59", "2026-11-16 07:00")]
    [TestCase("2026-11-16 07:00", "2026-11-16 13:00")]
    [TestCase("2026-11-16 13:00", "2026-11-17 07:00")]
    [TestCase("2026-11-16 23:30", "2026-11-17 07:00")]
    public void Daily_twice_a_day_takes_the_next_Kyiv_time(string now, string expected)
    {
        var schedule = TaskSchedule.Daily([new TimeOnly(13, 0), new TimeOnly(7, 0)], TaskSchedule.EveryDay).Value;
        Assert.That(schedule.NextAfter(Kyiv(now)), Is.EqualTo(Kyiv(expected)));
    }

    [TestCase("2026-11-13 09:00", "2026-11-16 09:00")] // Friday after the run → Monday
    [TestCase("2026-11-14 12:00", "2026-11-16 09:00")] // Saturday
    public void Weekdays_skip_the_weekend(string now, string expected)
    {
        var schedule = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));
        Assert.That(schedule.NextAfter(Kyiv(now)), Is.EqualTo(Kyiv(expected)));
    }

    [Test]
    public void Weekly_runs_on_Monday_only()
    {
        var schedule = TaskSchedule.DailyAt([DayOfWeek.Monday], new TimeOnly(6, 0));
        Assert.That(schedule.NextAfter(Kyiv("2026-11-16 06:00")), Is.EqualTo(Kyiv("2026-11-23 06:00")));
    }

    [TestCase("2026-11-16 07:10", "2026-11-16 08:00")]
    [TestCase("2026-11-16 08:00", "2026-11-16 09:00")]
    [TestCase("2026-11-16 20:00", "2026-11-17 08:00")]
    public void Interval_inside_a_window_runs_on_the_hour_grid(string now, string expected)
    {
        var schedule = TaskSchedule.Interval(60, 8, 20).Value;
        Assert.That(schedule.NextAfter(Kyiv(now)), Is.EqualTo(Kyiv(expected)));
    }

    [Test]
    public void Interval_without_a_window_starts_at_midnight()
    {
        var schedule = TaskSchedule.Interval(10).Value;
        Assert.That(schedule.NextAfter(Kyiv("2026-11-16 10:03")), Is.EqualTo(Kyiv("2026-11-16 10:10")));
        Assert.That(schedule.NextAfter(Kyiv("2026-11-16 23:55")), Is.EqualTo(Kyiv("2026-11-17 00:00")));
    }

    [Test]
    public void Kyiv_instants_are_UTC_because_PostgreSQL_timestamptz_accepts_only_UTC()
    {
        Assert.That(Kyiv("2026-11-16 09:00").Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(Kyiv("2026-07-16 09:00").Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(Kyiv("2026-11-16 09:00").UtcDateTime, Is.EqualTo(new DateTime(2026, 11, 16, 7, 0, 0)));
    }

    [Test]
    public void Daily_time_keeps_the_Kyiv_clock_across_the_autumn_change()
    {
        var schedule = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(3, 0));
        var beforeChange = schedule.NextAfter(Kyiv("2026-10-24 01:00"))!.Value;
        var changeDay = schedule.NextAfter(beforeChange)!.Value;
        var afterChange = schedule.NextAfter(changeDay)!.Value;
        Assert.That(beforeChange.UtcDateTime, Is.EqualTo(new DateTime(2026, 10, 24, 0, 0, 0)));
        Assert.That(KyivTime.ToLocal(changeDay), Is.EqualTo(new DateTime(2026, 10, 25, 3, 0, 0)), "03:00 happens twice: once");
        Assert.That(afterChange.UtcDateTime, Is.EqualTo(new DateTime(2026, 10, 26, 1, 0, 0)));
        Assert.That(KyivTime.ToLocal(afterChange), Is.EqualTo(new DateTime(2026, 10, 26, 3, 0, 0)));
    }

    [Test]
    public void Manual_has_no_next_run() =>
        Assert.That(TaskSchedule.Manual.NextAfter(DateTimeOffset.UtcNow), Is.Null);

    [TestCase(0, null, null)]
    [TestCase(1441, null, null)]
    [TestCase(60, 8, null)]
    [TestCase(60, 20, 8)]
    [TestCase(60, 8, 24)]
    public void Invalid_interval_is_refused(int minutes, int? from, int? to) =>
        Assert.That(TaskSchedule.Interval(minutes, from, to).IsFailure, Is.True);

    [Test]
    public void Daily_needs_times_and_days()
    {
        Assert.That(TaskSchedule.Daily([], TaskSchedule.EveryDay).Error, Is.EqualTo(ScheduleErrors.TimesInvalid));
        Assert.That(TaskSchedule.Daily([new TimeOnly(9, 0)], []).Error, Is.EqualTo(ScheduleErrors.DaysInvalid));
        var seven = Enumerable.Range(0, 7).Select(h => new TimeOnly(h, 0));
        Assert.That(TaskSchedule.Daily(seven, TaskSchedule.EveryDay).Error, Is.EqualTo(ScheduleErrors.TimesInvalid));
    }

    [Test]
    public void Equal_schedules_compare_by_value() =>
        Assert.That(TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0)),
            Is.EqualTo(TaskSchedule.Daily([new TimeOnly(9, 0)], TaskSchedule.Weekdays).Value));
}
