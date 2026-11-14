using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Features.Catalog.Shared;
using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;
using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace UnitTests.Scheduling.Application;

/// <summary>SCH-001 SC-07 (schedule), SC-08 (run now) and the page list.</summary>
public sealed class ScheduledTaskAdminTests
{
    private static readonly ScheduleInput TwiceADay = new("daily", Times: ["07:00", "13:00"], Days: [1, 2, 3, 4, 5, 6, 7]);

    private static async Task<Result<ScheduledTaskResponse>> UpdateAsync(SchedulingTestHarness harness, string key,
        ScheduledTaskScheduleInput input)
    {
        await using var db = harness.NewContext();
        return await new UpdateScheduledTaskScheduleHandler(db, harness.Time)
            .Handle(new UpdateScheduledTaskScheduleCommand(key, input, "Олена"), CancellationToken.None);
    }

    private static async Task<Result<ScheduledTaskResponse>> RunNowAsync(SchedulingTestHarness harness, string key)
    {
        await using var db = harness.NewContext();
        return await new RunScheduledTaskNowHandler(db, harness.Time)
            .Handle(new RunScheduledTaskNowCommand(key, "Олена"), CancellationToken.None);
    }

    [Test]
    public async Task Page_lists_every_task_in_catalogue_order_with_names()
    {
        using var harness = new SchedulingTestHarness();

        var tasks = (await new GetScheduledTasksHandler(harness.Db, harness.Time)
            .Handle(new GetScheduledTasksQuery(), CancellationToken.None)).Value;

        Assert.That(tasks.Select(x => x.Key), Is.EqualTo(ScheduledTaskCatalog.Tasks.Select(x => x.Key)));
        Assert.That(tasks.All(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Description)), Is.True);
        Assert.That(tasks.Single(x => x.Key == ScheduledTaskKeys.Outbox).Editable, Is.False);
        Assert.That(tasks.Any(x => x.LastRunFailed || x.Overdue), Is.False);
    }

    [Test]
    public async Task Admin_changes_the_schedule_and_the_next_run_follows_it()
    {
        using var harness = new SchedulingTestHarness();

        var result = await UpdateAsync(harness, ScheduledTaskKeys.Leads, new(true, TwiceADay));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Enabled, Is.True);
        Assert.That(result.Value.Schedule.Times, Is.EqualTo(new[] { "07:00", "13:00" }));
        Assert.That(result.Value.NextRunAt, Is.EqualTo(KyivTime.ToInstant(new DateTime(2026, 11, 16, 13, 0, 0))));
        var saved = await harness.NewContext().Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Leads);
        Assert.That(saved.UpdatedBy, Is.EqualTo("Олена"));
    }

    [Test]
    public async Task Admin_moves_the_next_run_once()
    {
        using var harness = new SchedulingTestHarness();
        var tomorrow = harness.Time.GetUtcNow().AddDays(1);

        var moved = await UpdateAsync(harness, ScheduledTaskKeys.Leads, new(true, TwiceADay, tomorrow));
        var past = await UpdateAsync(harness, ScheduledTaskKeys.Leads, new(true, TwiceADay, harness.Time.GetUtcNow().AddHours(-1)));

        Assert.That(moved.Value.NextRunAt, Is.EqualTo(tomorrow));
        Assert.That(past.Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public async Task Outbox_schedule_cannot_be_changed_or_run_now()
    {
        using var harness = new SchedulingTestHarness();

        var update = await UpdateAsync(harness, ScheduledTaskKeys.Outbox, new(false, new ScheduleInput("manual")));
        var run = await RunNowAsync(harness, ScheduledTaskKeys.Outbox);

        Assert.That(update.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(run.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Unknown_task_is_not_found()
    {
        using var harness = new SchedulingTestHarness();

        Assert.That((await RunNowAsync(harness, "nope")).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await UpdateAsync(harness, "nope", new(true, TwiceADay))).Status, Is.EqualTo(ResultStatus.NotFound));
        var runs = await new GetScheduledTaskRunsHandler(harness.Db).Handle(new GetScheduledTaskRunsQuery("nope"), CancellationToken.None);
        Assert.That(runs.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Run_now_starts_a_disabled_task_on_the_next_tick_once()
    {
        using var harness = new SchedulingTestHarness();

        var requested = await RunNowAsync(harness, ScheduledTaskKeys.Leads);
        var again = await RunNowAsync(harness, ScheduledTaskKeys.Leads);
        var tick = await harness.TickAsync();
        var whileRunning = await RunNowAsync(harness, ScheduledTaskKeys.Leads);

        Assert.That(requested.Value.RunRequestedAt, Is.EqualTo(harness.Time.GetUtcNow()));
        Assert.That(again.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(tick.Started, Is.EqualTo(new[] { ScheduledTaskKeys.Leads }));
        Assert.That(whileRunning.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(whileRunning.Errors, Has.Some.Contains("вже виконується"));

        var runs = (await new GetScheduledTaskRunsHandler(harness.Db)
            .Handle(new GetScheduledTaskRunsQuery(ScheduledTaskKeys.Leads), CancellationToken.None)).Value;
        Assert.That(runs.Single().Trigger, Is.EqualTo("manual"));
        Assert.That(runs.Single().RequestedBy, Is.EqualTo("Олена"));
    }

    [Test]
    public async Task Outbox_row_saved_with_an_older_schedule_follows_the_catalogue()
    {
        using var harness = new SchedulingTestHarness();
        harness.Db.Tasks.Add(ScheduledTask.Create(ScheduledTaskKeys.Outbox, TaskSchedule.Interval(1).Value, true,
            harness.Time.GetUtcNow()));
        await harness.Db.SaveChangesAsync();

        await using var db = harness.NewContext();
        var tasks = (await new GetScheduledTasksHandler(db, harness.Time)
            .Handle(new GetScheduledTasksQuery(), CancellationToken.None)).Value;

        var outbox = tasks.Single(x => x.Key == ScheduledTaskKeys.Outbox);
        Assert.That(outbox.Schedule.EveryMinutes, Is.EqualTo(5));
        Assert.That(outbox.Enabled, Is.True);
        var saved = await harness.NewContext().Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Outbox);
        Assert.That(saved.Schedule, Is.EqualTo(TaskSchedule.Interval(5).Value));
    }
}
