using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Queries.GetTasks;

/// <summary>A page of tasks: overdue first, then urgent, then by due time; done ones newest first.</summary>
/// <param name="AssigneeUserId">Director/Admin filter by manager; a Manager always gets only their own.</param>
/// <param name="Filter">A <see cref="TaskListFilter"/> name; `Open` when empty.</param>
/// <param name="CustomerPublicId">The client card: only this client's tasks (CARD-001 R-05).</param>
/// <param name="From">First Kyiv day of creation, inclusive.</param>
/// <param name="To">Last Kyiv day of creation, inclusive.</param>
public sealed record GetTasksQuery(TaskActor Actor, string? Search, string? Filter, int? AssigneeUserId,
    int Page = 1, int PageSize = 25, Guid? CustomerPublicId = null, DateOnly? From = null, DateOnly? To = null)
    : IQuery<Result<TaskPageResponse>>;
