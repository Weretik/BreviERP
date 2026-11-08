using Crm.Application.Tasks.Contracts;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.Mapping;

internal static class TaskResponseMapper
{
    /// <summary>SQL-translatable projection for lists and the board.</summary>
    public static IQueryable<TaskSummaryResponse> ToSummaries(this IQueryable<ManagerTask> tasks, DateTimeOffset now) =>
        tasks.Select(x => new TaskSummaryResponse(
            x.Id,
            "",
            x.Title,
            x.Status.ToString(),
            "",
            x.IsUrgent,
            x.Status != ManagerTaskStatus.Done && x.DueAt < now,
            x.DueAt,
            new TaskPersonResponse(x.AssigneeUserId, x.AssigneeName),
            x.CustomerName,
            x.Steps.Count,
            x.Steps.Count(s => s.IsDone),
            x.CompletedAt,
            x.UpdatedAt,
            x.Version));

    /// <summary>Fills the number and lane after the SQL read.</summary>
    public static TaskSummaryResponse Complete(this TaskSummaryResponse row, DateTimeOffset now) =>
        row with
        {
            Number = ManagerTask.NumberOf(row.Id),
            Lane = ManagerTask.LaneOf(Enum.Parse<ManagerTaskStatus>(row.Status), row.DueAt, row.IsUrgent, now)
                .ToString(),
        };

    public static TaskDetailResponse ToDetail(this ManagerTask task, DateTimeOffset now) =>
        new(task.Id,
            ManagerTask.NumberOf(task.Id),
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.LaneAt(now).ToString(),
            task.IsUrgent,
            task.LaneAt(now) == ManagerTaskLane.Overdue,
            task.DueAt,
            new TaskPersonResponse(task.AssigneeUserId, task.AssigneeName),
            new TaskPersonResponse(task.AuthorUserId, task.AuthorName),
            task.CustomerPublicId,
            task.CustomerName,
            task.Steps.OrderBy(x => x.Position).Select(x => new TaskStepResponse(x.Id, x.Position, x.Text, x.IsDone))
                .ToArray(),
            task.Events.OrderByDescending(x => x.At).ThenByDescending(x => x.Id)
                .Select(x => new TaskEventResponse(x.Kind.ToString(), new TaskPersonResponse(x.ByUserId, x.ByName),
                    x.At, x.Details))
                .ToArray(),
            task.CompletedAt,
            task.CreatedAt,
            task.UpdatedAt,
            task.Version,
            task.LeadId);
}
