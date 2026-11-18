using Crm.Domain.Leads.Identifiers;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Domain.Tasks.Enums;

namespace Crm.Application.Features.Tasks.Shared.Mapping;

internal static class TaskResponseMapper
{
    /// <summary>SQL-translatable projection for lists and the board.</summary>
    public static IQueryable<TaskSummaryResponse> ToSummaries(this IQueryable<ManagerTask> tasks, DateTimeOffset now) =>
        tasks.Select(x => new TaskSummaryResponse(
            x.Id.Value,
            "",
            x.Title,
            x.Status.ToString(),
            "",
            x.IsUrgent,
            x.Status != ManagerTaskStatus.Done && x.DueAt < now,
            x.DueAt,
            new TaskPersonResponse(x.AssigneeUserId.Value, x.AssigneeName),
            x.CustomerName,
            x.Steps.Count,
            x.Steps.Count(s => s.IsDone),
            x.CompletedAt,
            x.UpdatedAt,
            x.Version,
            x.CreatedAt));

    /// <summary>Fills the number and lane after the SQL read.</summary>
    public static TaskSummaryResponse Complete(this TaskSummaryResponse row, DateTimeOffset now) =>
        row with
        {
            Number = ManagerTask.NumberOf(row.Id),
            Lane = ManagerTask.LaneOf(Enum.Parse<ManagerTaskStatus>(row.Status), row.DueAt, row.IsUrgent, now)
                .ToString(),
        };

    /// <summary>The card of a task with its saved history, read by a query after the change is saved.</summary>
    public static async Task<TaskDetailResponse> DetailAsync(this ITaskStore store, ManagerTask task, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var events = await store.ManagerTaskEvents.Where(x => x.TaskId == task.Id)
            .OrderByDescending(x => x.At).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        return task.ToDetail(events, now);
    }

    private static TaskDetailResponse ToDetail(this ManagerTask task, IReadOnlyList<ManagerTaskEvent> events,
        DateTimeOffset now) =>
        new(task.Id.Value,
            ManagerTask.NumberOf(task.Id.Value),
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.LaneAt(now).ToString(),
            task.IsUrgent,
            task.LaneAt(now) == ManagerTaskLane.Overdue,
            task.DueAt,
            new TaskPersonResponse(task.AssigneeUserId.Value, task.AssigneeName),
            new TaskPersonResponse(task.AuthorUserId.Value, task.AuthorName),
            task.CustomerPublicId,
            task.CustomerName,
            task.Steps.OrderBy(x => x.Position).Select(x => new TaskStepResponse(x.Id, x.Position, x.Text, x.IsDone))
                .ToArray(),
            events.Select(x => new TaskEventResponse(x.Kind.ToString(), new TaskPersonResponse(x.ByUserId.Value, x.ByName),
                    x.At, x.Details))
                .ToArray(),
            task.CompletedAt,
            task.CreatedAt,
            task.UpdatedAt,
            task.Version,
            task.LeadId?.Value);
}
