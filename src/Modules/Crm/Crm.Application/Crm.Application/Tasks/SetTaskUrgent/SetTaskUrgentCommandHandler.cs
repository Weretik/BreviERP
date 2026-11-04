using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Building;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Application.Tasks.Staff;

namespace Crm.Application.Tasks.SetTaskUrgent;

public sealed class SetTaskUrgentCommandHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<SetTaskUrgentCommand, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(SetTaskUrgentCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden(TaskMessages.AdminOnly);
        var task = await store.LoadAsync(command.Id, cancellationToken);
        if (task is null) return Result.NotFound(TaskMessages.NotFound);
        if (task.Version != command.Version) return Result.Conflict(TaskMessages.Stale);

        var by = await staff.PersonAsync(command.Actor, cancellationToken);
        if (by is null) return Result.Forbidden(TaskMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        if (!task.SetUrgent(command.IsUrgent, by, now)) return Result.Success(task.ToDetail(now));
        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(task.ToDetail(now))
            : Result.Conflict(TaskMessages.Stale);
    }
}
