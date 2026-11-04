using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.EditTask;

/// <summary>Director or Admin saves the side panel of an existing task (R-02, R-12).</summary>
public sealed record EditTaskCommand(TaskActor Actor, long Id, SaveTaskInput Input) : ICommand<Result<TaskDetailResponse>>;
