using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.ChangeTaskStatus;

/// <summary>The manager (or Director/Admin) moves a task: `New`, `InProgress` or `Done` (R-07).</summary>
public sealed record ChangeTaskStatusCommand(TaskActor Actor, long Id, int Version, string Status)
    : ICommand<Result<TaskDetailResponse>>;
