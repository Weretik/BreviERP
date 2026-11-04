using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Building;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Application.Tasks.Staff;

namespace Crm.Application.Tasks.MarkTaskStep;

public sealed class MarkTaskStepCommandHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
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
