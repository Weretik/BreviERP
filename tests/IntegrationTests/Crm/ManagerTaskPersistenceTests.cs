using Ardalis.Result;
using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.ChangeTaskStatus;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.CreateTask;
using Crm.Application.Tasks.DeleteTask;
using Crm.Application.Tasks.EditTask;
using Crm.Application.Tasks.GetTask;
using Crm.Application.Tasks.GetTaskBoard;
using Crm.Application.Tasks.GetTaskOverview;
using Crm.Application.Tasks.GetTasks;
using Crm.Application.Tasks.MarkTaskStep;
using Crm.Application.Tasks.SetTaskUrgent;
using Crm.Application.Tasks.Staff;

namespace IntegrationTests.Crm;

/// <summary>Manager task handlers over the real crm schema: access, lanes, versions, overview (TSK-001).</summary>
[NonParallelizable]
public sealed class ManagerTaskPersistenceTests : CrmDatabaseFixture
{
    private static readonly TaskActor Director = new(1, true);
    private static readonly TaskActor Olha = new(17, false);
    private static readonly TaskActor Iryna = new(18, false);

    private readonly Staff _staff = new();
    private readonly MovableClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));

    [SetUp]
    public void ResetClock() => _clock.Now = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private SaveTaskInput Input(int assignee = 17, int version = 0, string title = "Передзвонити «Агросвіт»",
        int dueInHours = 24, bool urgent = false, params TaskStepInput[] steps) =>
        new(version, title, null, assignee, _clock.Now.AddHours(dueInHours), urgent, null, steps);

    private async Task<TaskDetailResponse> Create(SaveTaskInput input)
    {
        await using var db = Context();
        var result = await new CreateTaskCommandHandler(db, _staff, _clock)
            .Handle(new CreateTaskCommand(Director, input), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage))));
        return result.Value;
    }

    private async Task<TaskPageResponse> List(TaskActor actor, string? filter = null, int? assignee = null,
        string? search = null)
    {
        await using var db = Context();
        return (await new GetTasksQueryHandler(db, _clock)
            .Handle(new GetTasksQuery(actor, search, filter, assignee), default)).Value;
    }

    private async Task<Result<TaskDetailResponse>> Status(TaskActor actor, TaskDetailResponse task, string status)
    {
        await using var db = Context();
        return await new ChangeTaskStatusCommandHandler(db, _staff, _clock)
            .Handle(new ChangeTaskStatusCommand(actor, task.Id, task.Version, status), default);
    }

    [Test]
    public async Task DirectorCreatesATaskWithStepsForAManager()
    {
        var task = await Create(Input(steps: [new("Розрахувати", true), new("Надіслати", false)]));

        Assert.That(task.Number, Is.EqualTo("ЗД-0001"));
        Assert.That(task.Assignee, Is.EqualTo(new TaskPersonResponse(17, "Ольга Коваль")));
        Assert.That(task.Author.Name, Is.EqualTo("Директор"));
        Assert.That(task.Status, Is.EqualTo("New"));
        Assert.That(task.Lane, Is.EqualTo("Active"));
        Assert.That(task.Steps.Select(x => (x.Text, x.IsDone)), Is.EqualTo(new[] { ("Розрахувати", true), ("Надіслати", false) }));
        Assert.That(task.History.Single().Kind, Is.EqualTo("Created"));
    }

    [Test]
    public async Task OnlyAnActiveManagerCanReceiveATaskAndOnlyAdminGivesIt()
    {
        await using var db = Context();
        var handler = new CreateTaskCommandHandler(db, _staff, _clock);

        var toDirector = await handler.Handle(new CreateTaskCommand(Director, Input(assignee: 1)), default);
        var byManager = await handler.Handle(new CreateTaskCommand(Olha, Input()), default);
        var unknownClient = await handler.Handle(new CreateTaskCommand(Director,
            Input() with { CustomerPublicId = Guid.NewGuid() }), default);

        Assert.That(toDirector.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
        Assert.That(unknownClient.Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public async Task AManagerSeesAndMovesOnlyOwnTasks()
    {
        var olhas = await Create(Input(17));
        await Create(Input(18, title: "Ірині"));

        var list = await List(Olha, assignee: 18);
        var foreign = await Status(Iryna, olhas, "InProgress");
        var own = await Status(Olha, olhas, "InProgress");

        Assert.That(list.Items.Select(x => x.Id), Is.EqualTo(new[] { olhas.Id }));
        Assert.That(foreign.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(own.Value.Status, Is.EqualTo("InProgress"));
        Assert.That(own.Value.History[0].Kind, Is.EqualTo("StatusChanged"));
    }

    [Test]
    public async Task LanesFollowDueTimeUrgencyAndStatus()
    {
        var overdueUrgent = await Create(Input(dueInHours: 1, urgent: true, title: "A"));
        var urgent = await Create(Input(dueInHours: 48, urgent: true, title: "B"));
        var active = await Create(Input(dueInHours: 72, title: "C"));
        var done = await Status(Olha, await Create(Input(dueInHours: 5, title: "D")), "Done");
        _clock.Now = _clock.Now.AddHours(2);

        await using var db = Context();
        var board = (await new GetTaskBoardQueryHandler(db, _clock).Handle(new GetTaskBoardQuery(Olha, null), default)).Value;

        Assert.That(board.Items.Select(x => (x.Title, x.Lane)), Is.EquivalentTo(new[]
        {
            ("A", "Overdue"), ("B", "Urgent"), ("C", "Active"), ("D", "Done"),
        }));
        Assert.That(board.Items.Last().Title, Is.EqualTo("D"));
        Assert.That(board.Items.Single(x => x.Id == overdueUrgent.Id).IsOverdue, Is.True);
        Assert.That(done.Value.CompletedAt, Is.Not.Null);
        Assert.That((await List(Olha)).Items.First().Id, Is.EqualTo(overdueUrgent.Id), "overdue first");
        Assert.That((await List(Olha, "Urgent")).Items.Select(x => x.Id), Is.EquivalentTo(new[] { overdueUrgent.Id, urgent.Id }));
        Assert.That((await List(Olha, "Done")).Items.Single().Title, Is.EqualTo("D"));
        Assert.That((await List(Olha, search: "зд-0003")).Items.Single().Id, Is.EqualTo(active.Id));
    }

    [Test]
    public async Task DoneTasksLeaveTheBoardAfterAWeek()
    {
        var task = await Create(Input());
        await Status(Olha, task, "Done");
        _clock.Now = _clock.Now.AddDays(8);

        await using var db = Context();
        var board = (await new GetTaskBoardQueryHandler(db, _clock).Handle(new GetTaskBoardQuery(Director, null), default)).Value;
        Assert.That(board.Items, Is.Empty);
    }

    [Test]
    public async Task EditKeepsHistoryAndRejectsAStaleVersion()
    {
        var task = await Create(Input());
        await using (var db = Context())
        {
            var edited = await new EditTaskCommandHandler(db, _staff, _clock).Handle(new EditTaskCommand(Director, task.Id,
                Input(18, task.Version, "Нова назва", 5, true)), default);
            Assert.That(edited.Value.Assignee.Name, Is.EqualTo("Ірина Мельник"));
            Assert.That(edited.Value.History.Select(x => x.Kind),
                Is.SupersetOf(new[] { "Edited", "Reassigned", "DueChanged", "UrgentChanged", "Created" }));
        }

        await using (var db = Context())
        {
            var stale = await new EditTaskCommandHandler(db, _staff, _clock)
                .Handle(new EditTaskCommand(Director, task.Id, Input(17, task.Version)), default);
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        }

        await using (var db = Context())
        {
            var olha = await new GetTaskQueryHandler(db, _clock).Handle(new GetTaskQuery(Olha, task.Id), default);
            Assert.That(olha.Status, Is.EqualTo(ResultStatus.NotFound), "given to Iryna now");
        }
    }

    [Test]
    public async Task StepsUrgencyAndDelete()
    {
        var task = await Create(Input(steps: [new("Крок", false)]));
        TaskDetailResponse marked;
        await using (var db = Context())
        {
            marked = (await new MarkTaskStepCommandHandler(db, _staff, _clock)
                .Handle(new MarkTaskStepCommand(Olha, task.Id, task.Steps[0].Id, task.Version, true), default)).Value;
        }

        Assert.That(marked.Steps[0].IsDone, Is.True);
        Assert.That(marked.Version, Is.EqualTo(task.Version + 1));

        await using (var db = Context())
        {
            var byManager = await new SetTaskUrgentCommandHandler(db, _staff, _clock)
                .Handle(new SetTaskUrgentCommand(Olha, task.Id, marked.Version, true), default);
            var byDirector = await new SetTaskUrgentCommandHandler(db, _staff, _clock)
                .Handle(new SetTaskUrgentCommand(Director, task.Id, marked.Version, true), default);
            Assert.That(byManager.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(byDirector.Value.Lane, Is.EqualTo("Urgent"));
            marked = byDirector.Value;
        }

        await using (var db = Context())
        {
            var deleted = await new DeleteTaskCommandHandler(db)
                .Handle(new DeleteTaskCommand(Director, task.Id, marked.Version), default);
            Assert.That(deleted.IsSuccess, Is.True);
        }

        Assert.That((await List(Director)).Total, Is.Zero);
    }

    [Test]
    public async Task OverviewCountsAndWorkloadPerManager()
    {
        await Create(Input(17, dueInHours: 1, urgent: true));
        var done = await Create(Input(17, dueInHours: 10));
        await Create(Input(18));
        await Status(Olha, done, "Done");
        _clock.Now = _clock.Now.AddHours(2);

        await using var db = Context();
        var handler = new GetTaskOverviewQueryHandler(db, _staff, _clock);
        var all = (await handler.Handle(new GetTaskOverviewQuery(Director, null), default)).Value;
        var olha = (await handler.Handle(new GetTaskOverviewQuery(Olha, 18), default)).Value;

        Assert.That((all.Open, all.Overdue, all.Urgent, all.Done, all.DoneLastWeek, all.OnTimePercent),
            Is.EqualTo((2, 1, 1, 1, 1, (int?)100)));
        Assert.That(all.Load.Select(x => (x.Name, x.Open, x.Overdue, x.DoneLastWeek, x.OnTimePercent)),
            Is.EqualTo(new[]
            {
                ("Андрій Бондар", 0, 0, 0, (int?)null), ("Ірина Мельник", 1, 0, 0, null),
                ("Ольга Коваль", 1, 1, 1, 100),
            }), "done and on-time count per manager for the «Навантаження» tab");
        Assert.That(olha.Open, Is.EqualTo(1), "a Manager's filter is ignored");
        Assert.That(olha.Load.Single().UserId, Is.EqualTo(17));
    }

    private sealed class Staff : ITaskStaffReader
    {
        private static readonly TaskStaffMember Director = new(1, "Директор", null);

        private static readonly TaskStaffMember[] Managers =
            [new(19, "Андрій Бондар", null), new(18, "Ірина Мельник", null), new(17, "Ольга Коваль", null)];

        public Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaskStaffMember>>(Managers);

        public Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken) =>
            Task.FromResult(Managers.FirstOrDefault(x => x.UserId == userId));

        public Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
            Task.FromResult(userId == 1 ? Director : Managers.FirstOrDefault(x => x.UserId == userId));
    }

    private sealed class MovableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
