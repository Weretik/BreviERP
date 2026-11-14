using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Building;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.MarkTaskStep;

public sealed class MarkTaskStepHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<MarkTaskStepCommand, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(MarkTaskStepCommand command,
        CancellationToken cancellationToken)
    {
        var task = await store.LoadAsync(command.Id, cancellationToken);
        if (task is null || !command.Actor.CanAccess(task)) return Result.NotFound(TaskMessages.NotFound);
        if (task.Version != command.Version) return Result.Conflict(TaskMessages.Stale);

        var by = await staff.PersonAsync(command.Actor, cancellationToken);
        if (by is null) return Result.Forbidden(TaskMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var version = task.Version;
        if (!task.MarkStep(command.StepId, command.IsDone, by, now)) return Result.NotFound(TaskMessages.StepNotFound);
        if (task.Version == version) return Result.Success(task.ToDetail(now));
        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(task.ToDetail(now))
            : Result.Conflict(TaskMessages.Stale);
    }
}
