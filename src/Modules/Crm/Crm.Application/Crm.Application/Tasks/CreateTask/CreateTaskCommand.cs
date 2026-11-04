using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.CreateTask;

/// <summary>Director or Admin gives a new task to a manager (R-02).</summary>
public sealed record CreateTaskCommand(TaskActor Actor, SaveTaskInput Input) : ICommand<Result<TaskDetailResponse>>;
