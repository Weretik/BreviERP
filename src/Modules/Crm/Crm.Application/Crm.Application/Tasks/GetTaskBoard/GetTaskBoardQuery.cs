using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.GetTaskBoard;

/// <summary>«Мої завдання» board: open tasks and those done in the last 7 days (R-06).</summary>
/// <param name="AssigneeUserId">Director/Admin manager filter; empty = all managers. Ignored for a Manager.</param>
public sealed record GetTaskBoardQuery(TaskActor Actor, int? AssigneeUserId) : IQuery<Result<TaskBoardResponse>>;
