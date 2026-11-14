using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Features.Catalog.Shared;
using Scheduling.Application.Features.Runs.Commands.StartScheduledTaskRun;
using Scheduling.Application.Features.Tasks.Shared;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace UnitTests.Scheduling.Application;

/// <summary>SCH-001 SC-01, SC-02, SC-05, SC-06 and the outbox inside the tick.</summary>
public sealed class SchedulerTickTests
{
    private static async Task EnableAsync(SchedulingTestHarness harness, string key, TaskSchedule schedule, DateTimeOffset nextRunAt)
    {
        var task = (await ScheduledTaskRows.LoadAsync(harness.Db, harness.Time.GetUtcNow(), CancellationToken.None))
            .Single(x => x.Task.Key == key).Task;
        Assert.That(task.ChangeSchedule(true, schedule, null, "admin", harness.Time.GetUtcNow()).IsSuccess, Is.True);
        harness.Db.Entry(task).Property(x => x.NextRunAt).CurrentValue = nextRunAt;
        await harness.Db.SaveChangesAsync();
    }

    [Test]
    public async Task First_tick_creates_every_catalogue_task_with_its_defaults()
    {
        using var harness = new SchedulingTestHarness();

        await harness.TickAsync();

        var tasks = await harness.Db.Tasks.AsNoTracking().ToListAsync();
        Assert.That(tasks.Select(x => x.Key), Is.EquivalentTo(ScheduledTaskCatalog.Tasks.Select(x => x.Key)));
        Assert.That(tasks.Where(x => x.Key != ScheduledTaskKeys.Outbox).All(x => !x.Enabled), Is.True, "R-12: old jobs first");
        Assert.That(harness.Launcher.Launches, Is.Empty);
    }

    [Test]
    public async Task Due_tasks_start_in_one_job_execution_and_only_once()
    {
        using var harness = new SchedulingTestHarness();
        var daily = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Leads, daily, harness.Time.GetUtcNow().AddMinutes(-1));
        await EnableAsync(harness, ScheduledTaskKeys.Reminders, daily, harness.Time.GetUtcNow().AddMinutes(-1));

        var first = await harness.TickAsync();
        harness.Time.Advance(TimeSpan.FromMinutes(1));
        var second = await harness.TickAsync();

        Assert.That(first.Started, Is.EquivalentTo(new[] { ScheduledTaskKeys.Leads, ScheduledTaskKeys.Reminders }));
        Assert.That(second.Started, Is.Empty, "a running task is not claimed again");
        Assert.That(harness.Launcher.Launches, Has.Count.EqualTo(1));
        Assert.That(harness.Launcher.Launches[0].Select(x => x.Key), Is.EquivalentTo(first.Started));
    }

    [Test]
    public async Task Finished_run_records_success_and_the_next_time()
    {
        using var harness = new SchedulingTestHarness();
        var twiceADay = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Leads, twiceADay, harness.Time.GetUtcNow());
        await harness.TickAsync();
        var launch = harness.Launcher.Launches.Single().Single();

        harness.Time.Advance(TimeSpan.FromMinutes(3));
        await harness.StartAndFinishAsync(launch, true, "Рядків 10: додано 1.");

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);
        var run = await check.Runs.SingleAsync(x => x.Id == launch.RunId);
        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        Assert.That(task.LastSuccessSummary, Is.EqualTo("Рядків 10: додано 1."));
        Assert.That(task.NextRunAt, Is.EqualTo(KyivTime.ToInstant(new DateTime(2026, 11, 16, 13, 0, 0))));
        Assert.That(run.Outcome, Is.EqualTo(RunOutcome.Succeeded));
    }

    [Test]
    public async Task Failed_run_keeps_the_reason_and_the_last_success()
    {
        using var harness = new SchedulingTestHarness();
        var daily = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0));
        await EnableAsync(harness, ScheduledTaskKeys.Leads, daily, harness.Time.GetUtcNow());
        await harness.TickAsync();

        await harness.StartAndFinishAsync(harness.Launcher.Launches.Single().Single(), false, "Таблицю не прочитано.");

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);
        Assert.That(task.LastFailureReason, Is.EqualTo("Таблицю не прочитано."));
        Assert.That(task.LastFailureAt, Is.EqualTo(harness.Time.GetUtcNow()));
        Assert.That(task.LastSuccessAt, Is.Null);
        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
    }

    [Test]
    public async Task Job_that_cannot_start_fails_the_tasks_with_the_reason()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Leads, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        harness.Launcher.Error = "немає прав";

        var tick = await harness.TickAsync();

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);
        Assert.That(tick.Started, Is.Empty);
        Assert.That(tick.LaunchError, Is.EqualTo("немає прав"));
        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
        Assert.That(task.LastFailureReason, Is.EqualTo("Не вдалося запустити задачу: немає прав"));
        Assert.That(task.NextRunAt, Is.GreaterThan(harness.Time.GetUtcNow()));
    }

    [Test]
    public async Task Stuck_run_fails_after_its_limit_and_the_task_runs_again()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Leads, TaskSchedule.Interval(60).Value, harness.Time.GetUtcNow());
        await harness.TickAsync();
        await using (var job = harness.NewContext())
        {
            var launch = harness.Launcher.Launches.Single().Single();
            Assert.That((await new StartScheduledTaskRunHandler(job, harness.Time)
                .Handle(new StartScheduledTaskRunCommand(launch.Key, launch.RunId), CancellationToken.None)).IsSuccess, Is.True);
        }

        harness.Time.Advance(TimeSpan.FromMinutes(31));
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var task = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);
        Assert.That(task.LastFailureReason, Is.EqualTo("Задача не завершилася за 30 хв."));
        Assert.That(task.State, Is.EqualTo(TaskState.Idle));
    }

    [Test]
    public async Task Outbox_runs_inside_every_tick_and_records_a_failure()
    {
        using var harness = new SchedulingTestHarness();
        await harness.TickAsync();
        harness.Outbox.Failure = "Telegram недоступний";
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var outbox = await check.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Outbox);
        Assert.That(harness.Outbox.Calls, Is.EqualTo(2));
        Assert.That(outbox.LastSuccessSummary, Is.EqualTo("Нічого досилати."));
        Assert.That(outbox.LastFailureReason, Is.EqualTo("Telegram недоступний"));
        Assert.That(harness.Launcher.Launches, Is.Empty, "the outbox never starts the job");
    }

    [Test]
    public async Task Two_ticks_cannot_claim_the_same_task()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Leads, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        await using var other = harness.NewContext();
        var seenByOther = await other.Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);

        await harness.TickAsync();
        other.Runs.Add(seenByOther.Claim(harness.Time.GetUtcNow()));

        Assert.That(async () => await other.SaveChangesAsync(), Throws.InstanceOf<DbUpdateConcurrencyException>());
    }

    [Test]
    public async Task Runs_older_than_ninety_days_are_deleted()
    {
        using var harness = new SchedulingTestHarness();
        await EnableAsync(harness, ScheduledTaskKeys.Leads, TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(9, 0)),
            harness.Time.GetUtcNow());
        await harness.TickAsync();
        await harness.StartAndFinishAsync(harness.Launcher.Launches.Single().Single(), true, "OK");

        harness.Time.Advance(TimeSpan.FromDays(91));
        harness.Launcher.Error = "вимкнено";
        await harness.TickAsync();

        await using var check = harness.NewContext();
        var runs = await check.Runs.AsNoTracking().ToListAsync();
        Assert.That(runs.All(x => x.ClaimedAt > harness.Time.GetUtcNow().AddDays(-90)), Is.True);
    }
}
