using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.GetTasks;

/// <summary>A page of tasks: overdue first, then urgent, then by due time; done ones newest first.</summary>
/// <param name="AssigneeUserId">Director/Admin filter by manager; a Manager always gets only their own.</param>
/// <param name="Filter">A <see cref="TaskListFilter"/> name; `Open` when empty.</param>
public sealed record GetTasksQuery(TaskActor Actor, string? Search, string? Filter, int? AssigneeUserId,
    int Page = 1, int PageSize = 25) : IQuery<Result<TaskPageResponse>>;
