using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Queries.GetTaskBoard;

/// <summary>«Мої завдання» board: every open task and the tasks done in the chosen period (R-06).</summary>
/// <param name="AssigneeUserId">Director/Admin manager filter; empty = all managers. Ignored for a Manager.</param>
/// <param name="Period">Which done tasks are shown, by the time they were done; «Цей тиждень» by default.</param>
public sealed record GetTaskBoardQuery(TaskActor Actor, int? AssigneeUserId,
    TaskBoardPeriod Period = TaskBoardPeriod.Week) : IQuery<Result<TaskBoardResponse>>;
