using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;

namespace Crm.Application.Tasks.MarkTaskStep;

/// <summary>The manager (or Director/Admin) ticks or unticks one step (R-10).</summary>
public sealed record MarkTaskStepCommand(TaskActor Actor, long Id, long StepId, int Version, bool IsDone)
    : ICommand<Result<TaskDetailResponse>>;
