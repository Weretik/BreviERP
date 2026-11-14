using Crm.Application.Features.Tasks.Shared;

namespace Crm.Application.Features.Tasks.Queries.GetTaskAssignees;

/// <summary>Managers a task can be given to and the manager filter lists (R-04); Director and Admin only.</summary>
public sealed record GetTaskAssigneesQuery : IQuery<Result<IReadOnlyList<TaskAssigneeResponse>>>;
