using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.GetTaskOverview;

/// <summary>Counts for the metric strip and chips, and the workload of each manager (R-09).</summary>
/// <param name="AssigneeUserId">Narrows the counts (not the workload) for Director/Admin; ignored for a Manager.</param>
public sealed record GetTaskOverviewQuery(TaskActor Actor, int? AssigneeUserId) : IQuery<Result<TaskOverviewResponse>>;
