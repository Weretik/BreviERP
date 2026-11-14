using Reminders.Domain.Calendar;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;

namespace UnitTests.Reminders;

public sealed class ReminderPlannerTests
{
    private static readonly WorkingCalendar Calendar = WorkingCalendar.WeekendsOnly;
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 7, 0, 0, TimeSpan.Zero);

    private static ReminderTask Task(bool withChat = true) => ReminderTask.Create("Декларація з ПДВ", ReminderOrganization.PpBrevi,
        new ReminderSchedule(ReminderPeriodicity.Monthly, DueRule.DayOfMonth(5, 1), 5),
        withChat ? new ReminderChatId(1) : null, new DateOnly(2026, 10, 1), true, Now).Value;

    private static ReminderOccurrence Occurrence(DateOnly due) =>
        ReminderOccurrence.Open(new ReminderTaskId(1), new ReminderPeriod(ReminderPeriodicity.Monthly, new DateOnly(2026, 10, 1)), due, Now);

    [TestCase("2026-10-30", false)] // Friday, window starts 31.10
    [TestCase("2026-10-31", false)] // Saturday
    [TestCase("2026-11-01", false)] // Sunday
    [TestCase("2026-11-02", true)]
    [TestCase("2026-11-05", true)]
    [TestCase("2026-11-09", true)]  // overdue, still reminded until done
    public void Reminds_on_working_days_inside_the_window(string today, bool expected) =>
        Assert.That(ReminderPlanner.ShouldRemind(Task(), Occurrence(new DateOnly(2026, 11, 5)), DateOnly.Parse(today), Calendar),
            Is.EqualTo(expected));

    [Test]
    public void Closed_postponed_or_chatless_periods_are_not_reminded()
    {
        var today = new DateOnly(2026, 11, 2);
        var closed = Occurrence(new DateOnly(2026, 11, 5));
        closed.Close(ReminderActor.Telegram(7, "Олена", null, null), Now);
        var postponed = Occurrence(new DateOnly(2026, 11, 5));
        postponed.Postpone(ReminderActor.Telegram(7, "Олена", null, null), Now, new DateOnly(2026, 11, 4));
        Assert.That(ReminderPlanner.ShouldRemind(Task(), closed, today, Calendar), Is.False);
        Assert.That(ReminderPlanner.ShouldRemind(Task(), postponed, today, Calendar), Is.False);
        Assert.That(ReminderPlanner.ShouldRemind(Task(), postponed, new DateOnly(2026, 11, 4), Calendar), Is.True);
        Assert.That(ReminderPlanner.ShouldRemind(Task(withChat: false), Occurrence(new DateOnly(2026, 11, 5)), today, Calendar), Is.False);
    }

    [Test]
    public void Friday_warns_about_a_weekend_deadline_and_later_days_count_overdue()
    {
        var sunday = new DateOnly(2026, 11, 15);
        Assert.That(ReminderPlanner.Notice(sunday, new DateOnly(2026, 11, 12), Calendar).Kind, Is.EqualTo(ReminderNoticeKind.None));
        Assert.That(ReminderPlanner.Notice(sunday, new DateOnly(2026, 11, 13), Calendar).Kind, Is.EqualTo(ReminderNoticeKind.DueOnNonWorkingDay));
        Assert.That(ReminderPlanner.Notice(sunday, new DateOnly(2026, 11, 16), Calendar), Is.EqualTo(new ReminderNotice(ReminderNoticeKind.Overdue, 1)));
        Assert.That(ReminderPlanner.Notice(new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 5), Calendar).Kind, Is.EqualTo(ReminderNoticeKind.DueToday));
    }

    [TestCase(SnoozeOption.Tomorrow, "2026-11-02", "2026-11-20", "2026-11-03")]
    [TestCase(SnoozeOption.TwoDays, "2026-11-02", "2026-11-20", "2026-11-04")]
    [TestCase(SnoozeOption.Monday, "2026-11-02", "2026-11-20", "2026-11-09")]
    [TestCase(SnoozeOption.Tomorrow, "2026-11-06", "2026-11-20", "2026-11-09")] // Friday → Monday
    [TestCase(SnoozeOption.Monday, "2026-11-02", "2026-11-05", "2026-11-05")]  // capped at the deadline
    [TestCase(SnoozeOption.Monday, "2026-11-12", "2026-11-15", "2026-11-13")]  // capped at Friday before a Sunday deadline
    [TestCase(SnoozeOption.TwoDays, "2026-11-09", "2026-11-05", "2026-11-11")] // overdue: no cap
    public void Snooze_counts_working_days_and_never_skips_the_deadline(SnoozeOption option, string today, string due, string expected) =>
        Assert.That(ReminderSnooze.Until(option, DateOnly.Parse(today), DateOnly.Parse(due), Calendar), Is.EqualTo(DateOnly.Parse(expected)));

    [Test]
    public void First_close_wins_and_reopen_records_the_journal()
    {
        var occurrence = Occurrence(new DateOnly(2026, 11, 5));
        Assert.That(occurrence.Close(ReminderActor.Telegram(1, "Олена", "Бух", "olena"), Now), Is.True);
        Assert.That(occurrence.Close(ReminderActor.Telegram(2, "Марія", null, null), Now.AddMinutes(1)), Is.False);
        Assert.That(occurrence.ClosedBy!.Name, Is.EqualTo("Олена Бух"));
        Assert.That(occurrence.Reopen(ReminderActor.Admin(5, "Віталій"), Now.AddHours(1)), Is.True);
        Assert.That(occurrence.Actions.Select(x => x.Kind), Is.EqualTo(new[] { ReminderActionKind.Done, ReminderActionKind.Reopened }));
    }

    [Test]
    public void Telegram_actor_without_a_name_uses_username_or_id()
    {
        Assert.That(ReminderActor.Telegram(9, null, null, "buh").Name, Is.EqualTo("@buh"));
        Assert.That(ReminderActor.Telegram(9, " ", null, null).Name, Is.EqualTo("id 9"));
    }
}
