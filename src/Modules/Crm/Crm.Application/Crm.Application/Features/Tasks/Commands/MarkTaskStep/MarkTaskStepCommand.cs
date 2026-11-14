using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;

namespace Crm.Application.Features.Tasks.Commands.MarkTaskStep;

/// <summary>The manager (or Director/Admin) ticks or unticks one step (R-10).</summary>
public sealed record MarkTaskStepCommand(TaskActor Actor, long Id, long StepId, int Version, bool IsDone)
    : ICommand<Result<TaskDetailResponse>>;
