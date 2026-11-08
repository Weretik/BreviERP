using Crm.Application.Tasks.GetTaskBoard;
using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.ChangeTaskStatus;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.CreateTask;
using Crm.Application.Tasks.GetTasks;
using Crm.Domain.Tasks;

namespace UnitTests.Crm.Tasks;

/// <summary>Lanes, statuses and side panel rules of manager tasks (TSK-001).</summary>
public sealed class ManagerTaskRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
    private static readonly ManagerTaskPerson Director = new(1, "Директор");
    private static readonly TaskActor Admin = new(1, true);

    private static ManagerTask Task(int dueInHours = 24, bool urgent = false) =>
        new(new ManagerTaskDraft(" Передзвонити ", "  ", new ManagerTaskPerson(17, "Ольга"), Now.AddHours(dueInHours),
            urgent, null, "ignored", [new ManagerTaskStepDraft(" Крок ", false)]), Director, Now);

    private static SaveTaskInput Input(string title = "Передзвонити", int assignee = 17, int steps = 1) =>
        new(0, title, null, assignee, Now, false, null,
            Enumerable.Range(0, steps).Select(i => new TaskStepInput($"Крок {i}", false)).ToArray());

    [Test]
    public void NumberHasThePrefixAndFourDigits()
    {
        Assert.That(ManagerTask.NumberOf(142), Is.EqualTo("ЗД-0142"));
        Assert.That(ManagerTask.NumberOf(12345), Is.EqualTo("ЗД-12345"));
    }

    [Test]
    public void OverdueWinsOverUrgentAndDoneWinsOverBoth()
    {
        Assert.That(Task(-1, urgent: true).LaneAt(Now), Is.EqualTo(ManagerTaskLane.Overdue));
        Assert.That(Task(1, urgent: true).LaneAt(Now), Is.EqualTo(ManagerTaskLane.Urgent));
        Assert.That(Task(1).LaneAt(Now), Is.EqualTo(ManagerTaskLane.Active));

        var done = Task(-1, urgent: true);
        done.ChangeStatus(ManagerTaskStatus.Done, Director, Now);
        Assert.That(done.LaneAt(Now), Is.EqualTo(ManagerTaskLane.Done));
    }

    [Test]
    public void NewTaskTrimsTextDropsEmptyDescriptionAndCustomerNameWithoutCustomer()
    {
        var task = Task();
        Assert.That(task.Title, Is.EqualTo("Передзвонити"));
        Assert.That(task.Description, Is.Null);
        Assert.That(task.CustomerName, Is.Null);
        Assert.That(task.Steps.Single().Text, Is.EqualTo("Крок"));
        Assert.That((task.Status, task.Version), Is.EqualTo((ManagerTaskStatus.New, 1)));
    }

    [Test]
    public void DoneSetsTheCompletionTimeAndReopeningClearsIt()
    {
        var task = Task();
        Assert.That(task.ChangeStatus(ManagerTaskStatus.Done, Director, Now), Is.True);
        Assert.That(task.CompletedAt, Is.EqualTo(Now));
        Assert.That(task.ChangeStatus(ManagerTaskStatus.Done, Director, Now), Is.False, "same status changes nothing");
        Assert.That(task.Version, Is.EqualTo(2));

        task.ChangeStatus(ManagerTaskStatus.InProgress, Director, Now);
        Assert.That(task.CompletedAt, Is.Null);
    }

    [Test]
    public void SidePanelRules()
    {
        var validator = new CreateTaskCommandValidator();
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input())).IsValid, Is.True);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input(title: "  "))).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input(title: new string('a', 201)))).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input(assignee: 0))).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input(steps: 31))).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin, Input() with { DueAt = default })).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateTaskCommand(Admin,
            Input() with { Steps = [new TaskStepInput(" ", false)] })).IsValid, Is.False);
    }

    [TestCase("InProgress", true)]
    [TestCase("Done", true)]
    [TestCase("1", false)]
    [TestCase("Overdue", false)]
    [TestCase("done", false)]
    public void OnlyKnownStatusNames(string status, bool valid) =>
        Assert.That(new ChangeTaskStatusCommandValidator()
            .Validate(new ChangeTaskStatusCommand(Admin, 1, 1, status)).IsValid, Is.EqualTo(valid));

    [TestCase(null, true)]
    [TestCase("Overdue", true)]
    [TestCase("All", false)]
    [TestCase("2", false)]
    public void OnlyKnownListFilters(string? filter, bool valid) =>
        Assert.That(new GetTasksQueryValidator().Validate(new GetTasksQuery(Admin, null, filter, null)).IsValid,
            Is.EqualTo(valid));

    [Test]
    public void BoardPeriodsFollowTheKyivWeek()
    {
        // Wednesday 2026-11-04 12:00 UTC; Kyiv is UTC+2 in November.
        var now = new DateTimeOffset(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);
        Assert.That(TaskBoardPeriods.Range(TaskBoardPeriod.Week, now),
            Is.EqualTo(((DateTimeOffset?)new DateTimeOffset(2026, 11, 1, 22, 0, 0, TimeSpan.Zero), (DateTimeOffset?)null)));
        Assert.That(TaskBoardPeriods.Range(TaskBoardPeriod.LastWeek, now),
            Is.EqualTo(((DateTimeOffset?)new DateTimeOffset(2026, 10, 25, 22, 0, 0, TimeSpan.Zero),
                (DateTimeOffset?)new DateTimeOffset(2026, 11, 1, 22, 0, 0, TimeSpan.Zero))));
        Assert.That(TaskBoardPeriods.Range(TaskBoardPeriod.All, now), Is.EqualTo(((DateTimeOffset?)null, (DateTimeOffset?)null)));
    }
}
