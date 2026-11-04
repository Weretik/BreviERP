using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.ChangeTaskStatus;

/// <summary>The manager (or Director/Admin) moves a task: `New`, `InProgress` or `Done` (R-07).</summary>
public sealed record ChangeTaskStatusCommand(TaskActor Actor, long Id, int Version, string Status)
    : ICommand<Result<TaskDetailResponse>>;
