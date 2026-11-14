using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.EditTask;

/// <summary>Director or Admin saves the side panel of an existing task (R-02, R-12).</summary>
public sealed record EditTaskCommand(TaskActor Actor, long Id, SaveTaskInput Input) : ICommand<Result<TaskDetailResponse>>;
