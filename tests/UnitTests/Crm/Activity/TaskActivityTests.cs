using BuildingBlocks.Domain.Identifiers.People;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Crm.Application.Features.Activity.EventHandlers;
using Crm.Domain.Tasks;
using Crm.Domain.Tasks.Enums;
using Crm.Domain.Tasks.ValueObjects;

namespace UnitTests.Crm.Activity;

/// <summary>Manager-task actions read as «Журнал дій» entries (ACT-001).</summary>
public sealed class TaskActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly ManagerTaskPerson Director = new(new UserId(1), "Директор");
    private readonly TaskActivity _describer = new();

    private static ManagerTaskDraft Draft(string title = "Передзвонити", int assignee = 17, string name = "Ольга") =>
        new(title, null, new ManagerTaskPerson(new UserId(assignee), name), Now.AddDays(1), false, null, null, []);

    private static ManagerTask CleanTask()
    {
        var task = ManagerTask.Create(Draft(), Director, Now);
        ((IHasDomainEvents)task).ClearDomainEvents();
        return task;
    }

    private ActivityDescription? Last(ManagerTask task) =>
        _describer.Describe(((IHasDomainEvents)task).DomainEvents.Last());

    [Test]
    public void Creating_names_the_assignee()
    {
        var task = ManagerTask.Create(Draft(), Director, Now);

        var entry = Last(task);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.Summary, Is.EqualTo("Виконавець: Ольга"));
    }

    [Test]
    public void Editing_lists_reassignment_and_new_title()
    {
        var task = CleanTask();

        task.Edit(Draft("Надіслати КП", 18, "Вадим"), Director, Now);

        Assert.That(Last(task)!.Changes, Is.EqualTo(new[]
        {
            new ActivityChangeItem("Назва", "Передзвонити", "Надіслати КП"),
            new ActivityChangeItem("Виконавець", "Ольга", "Вадим"),
        }));
    }

    [Test]
    public void Editing_without_changes_is_not_listed()
    {
        var task = CleanTask();

        task.Edit(Draft(), Director, Now);

        Assert.That(Last(task), Is.Null);
    }

    [Test]
    public void Status_change_and_delete_are_listed()
    {
        var task = CleanTask();

        task.ChangeStatus(ManagerTaskStatus.Done, Director, Now);
        Assert.That(Last(task)!.Summary, Is.EqualTo("Статус: Виконано"));

        task.MarkDeleted();
        Assert.That(Last(task)!.Action, Is.EqualTo(ActivityActions.Deleted));
    }
}
