using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace UnitTests.Scheduling.Domain;

public sealed class ScheduledTaskTests
{
    private static readonly DateTimeOffset Now = KyivTime.ToInstant(new DateTime(2026, 11, 16, 9, 0, 0));
    private static readonly TaskSchedule NineOnWeekdays = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));

    private static ScheduledTask DueTask() => ScheduledTask.Create("leads", NineOnWeekdays, enabled: true, Now.AddDays(-1));

    [Test]
    public void A_due_task_is_claimed_once()
    {
        var task = DueTask();
        Assert.That(task.IsDue(Now), Is.True);

        var run = task.Claim(Now);

        Assert.That(task.State, Is.EqualTo(TaskState.Running));
        Assert.That(task.CurrentRunId, Is.EqualTo(run.Id));
        Assert.That(run.Trigger, Is.EqualTo(RunTrigger.Schedule));
        Assert.That(task.IsDue(Now), Is.False);
        Assert.Throws<InvalidOperationException>(() => task.Claim(Now));
    }

    [Test]
    public void A_disabled_task_waits_until_run_now()
    {
        var task = ScheduledTask.Create("leads", NineOnWeekdays, enabled: false, Now.AddDays(-1));
        Assert.That(task.IsDue(Now), Is.False);

        Assert.That(task.RequestRun("director@brevi", Now).IsSuccess, Is.True);

        Assert.That(task.IsDue(Now), Is.True);
        var run = task.Claim(Now);
        Assert.That(run.Trigger, Is.EqualTo(RunTrigger.Manual));
        Assert.That(run.RequestedBy, Is.EqualTo("director@brevi"));
        Assert.That(task.RunRequestedAt, Is.Null);
    }

    [Test]
    public void Run_now_is_refused_while_running_or_already_requested()
    {
        var task = DueTask();
        Assert.That(task.RequestRun("a", Now).IsSuccess, Is.True);
        Assert.That(task.RequestRun("a", Now).Error, Is.EqualTo(ScheduledTaskErrors.AlreadyRequested));
        task.Claim(Now);
        Assert.That(task.RequestRun("a", Now).Error, Is.EqualTo(ScheduledTaskErrors.AlreadyRunning));
    }

    [Test]
    public void Success_records_the_summary_and_the_next_run()
    {
        var task = DueTask();
        var run = task.Claim(Now);
        task.Start(run, Now);

        Assert.That(task.Succeed(run, "Додано 3 ліди", Now.AddMinutes(2)).IsSuccess, Is.True);

        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        Assert.That(task.LastSuccessSummary, Is.EqualTo("Додано 3 ліди"));
        Assert.That(task.NextRunAt, Is.EqualTo(Now.AddDays(1)));
        Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Succeeded));
        Assert.That(run.StartedAt, Is.EqualTo(Now));
    }

    [Test]
    public void Failure_keeps_the_last_success_and_shortens_the_reason()
    {
        var task = DueTask();
        var first = task.Claim(Now);
        task.Succeed(first, "OK", Now);
        var second = task.Claim(Now.AddDays(1));

        task.Fail(second, new string('x', 900), Now.AddDays(1));

        Assert.That(task.LastSuccessAt, Is.EqualTo(Now));
        Assert.That(task.LastFailureReason!.Length, Is.EqualTo(ScheduledTaskErrors.MaxMessageLength));
        Assert.That(second.Outcome, Is.EqualTo(RunOutcome.Failed));
    }

    [Test]
    public void A_stale_run_changes_nothing()
    {
        var task = DueTask();
        var old = task.Claim(Now);
        task.Fail(old, "killed", Now);
        task.Claim(Now.AddDays(1));

        Assert.That(task.Succeed(old, "late", Now.AddDays(1)).Error, Is.EqualTo(ScheduledTaskErrors.StaleRun));
        Assert.That(task.State, Is.EqualTo(TaskState.Running));
    }

    [Test]
    public void A_stuck_run_expires_after_its_limit_since_it_started()
    {
        var task = DueTask();
        var run = task.Claim(Now);
        Assert.That(task.Start(run, Now.AddMinutes(20)).IsSuccess, Is.True, "waited for earlier tasks of the job");

        Assert.That(task.ExpireIfStuck(run, 30, Now.AddMinutes(49)), Is.False);
        Assert.That(task.ExpireIfStuck(run, 30, Now.AddMinutes(51)), Is.True);

        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        Assert.That(task.LastFailureReason, Is.EqualTo("Задача не завершилася за 30 хв."));
        Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Failed));
    }

    [Test]
    public void A_run_that_never_starts_expires_after_the_queue_limit()
    {
        var task = DueTask();
        var run = task.Claim(Now);

        Assert.That(task.ExpireIfStuck(run, 30, Now.AddMinutes(ScheduledTask.QueueLimitMinutes - 1)), Is.False);
        Assert.That(task.ExpireIfStuck(run, 30, Now.AddMinutes(ScheduledTask.QueueLimitMinutes + 1)), Is.True);
        Assert.That(task.LastFailureReason, Does.StartWith("Задача не почалася"));
    }

    [Test]
    public void Changing_the_schedule_recomputes_the_next_run()
    {
        var task = DueTask();
        var twice = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0));

        Assert.That(task.ChangeSchedule(true, twice, null, "admin", Now).IsSuccess, Is.True);

        Assert.That(task.NextRunAt, Is.EqualTo(Now.AddHours(4)));
        Assert.That(task.ChangeSchedule(true, twice, Now.AddMinutes(-1), "admin", Now).Error,
            Is.EqualTo(ScheduledTaskErrors.NextRunInPast));
    }

    [Test]
    public void Inline_work_records_results_without_a_run()
    {
        var task = ScheduledTask.Create("outbox", TaskSchedule.Interval(5).Value, true, Now);
        task.RecordInline(false, "Telegram недоступний", Now);
        Assert.That(task.LastFailureReason, Is.EqualTo("Telegram недоступний"));
        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
    }
}
