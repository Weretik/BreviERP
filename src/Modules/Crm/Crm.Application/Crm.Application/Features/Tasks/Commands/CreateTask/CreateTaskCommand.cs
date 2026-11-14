using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.CreateTask;

/// <summary>Director or Admin gives a new task to a manager (R-02).</summary>
public sealed record CreateTaskCommand(TaskActor Actor, SaveTaskInput Input) : ICommand<Result<TaskDetailResponse>>;
