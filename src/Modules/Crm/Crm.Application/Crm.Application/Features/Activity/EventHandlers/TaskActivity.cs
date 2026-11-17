using ActivityLog.Contracts.Activity;
using Crm.Domain.Tasks;
using Crm.Domain.Tasks.Enums;
using Crm.Domain.Tasks.ValueObjects;

namespace Crm.Application.Features.Activity.EventHandlers;

/// <summary>How manager-task actions read in «Журнал дій» (ACT-001).</summary>
public sealed class TaskActivity : IActivityDescriber
{
    public const string Subject = "task";
    public const string SubjectTitle = "Завдання менеджеру";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        ManagerTaskRecorded recorded => Recorded(recorded),
        ManagerTaskDeleted deleted => Of(ActivityActions.Deleted, deleted.TaskId, deleted.Title, null, []),
        _ => null,
    };

    private static ActivityDescription? Recorded(ManagerTaskRecorded e)
    {
        var task = e.Task;
        return e.Kind switch
        {
            ManagerTaskEventKind.Created => Of(ActivityActions.Created, task.Id.Value, task.Title,
                "Виконавець: " + task.AssigneeName, []),
            ManagerTaskEventKind.Edited => Changes(e.Before, task.Snapshot()) is { Any: true } changes
                ? Of(ActivityActions.Changed, task.Id.Value, task.Title, null, changes.Items)
                : null,
            ManagerTaskEventKind.StatusChanged => Of(ActivityActions.Changed, task.Id.Value, task.Title,
                "Статус: " + Status(task.Status), []),
            ManagerTaskEventKind.UrgentChanged => Of(ActivityActions.Changed, task.Id.Value, task.Title,
                task.IsUrgent ? "Позначено терміновим" : "Знято позначку «Терміново»", []),
            ManagerTaskEventKind.StepChecked => Of(ActivityActions.Changed, task.Id.Value, task.Title,
                "Виконано крок: " + e.Details, []),
            ManagerTaskEventKind.StepUnchecked => Of(ActivityActions.Changed, task.Id.Value, task.Title,
                "Знято позначку кроку: " + e.Details, []),
            _ => null,
        };
    }

    private static ActivityChanges Changes(ManagerTaskSnapshot? before, ManagerTaskSnapshot after)
    {
        var changes = new ActivityChanges();
        if (before is null) return changes;
        return changes
            .Add("Назва", before.Title, after.Title)
            .Add("Опис", Described(before.Description, after.Description, "попередній"),
                Described(after.Description, before.Description, "новий"))
            .Add("Виконавець", before.AssigneeName, after.AssigneeName)
            .Add("Термін", before.DueAt, after.DueAt)
            .Add("Терміново", before.IsUrgent, after.IsUrgent)
            .Add("Клієнт", before.CustomerName, after.CustomerName)
            .Add("Кроків", before.StepCount, after.StepCount);
    }

    /// <summary>A long description is not copied into the journal: only that it was written, changed or cleared.</summary>
    private static string? Described(string? value, string? other, string word) =>
        string.Equals(value, other, StringComparison.Ordinal) || value is null ? null : word + " текст";

    private static string Status(ManagerTaskStatus status) => status switch
    {
        ManagerTaskStatus.New => "Нове",
        ManagerTaskStatus.InProgress => "В роботі",
        _ => "Виконано",
    };

    private static ActivityDescription Of(string action, long id, string title, string? summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, SubjectTitle, id.ToString(CultureInfo.InvariantCulture),
            $"{ManagerTask.NumberOf(id)} · {title}", summary, changes, "Crm");
}
