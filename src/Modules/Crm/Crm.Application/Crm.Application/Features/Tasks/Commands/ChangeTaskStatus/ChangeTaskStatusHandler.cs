using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Building;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<ChangeTaskStatusCommand, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(ChangeTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var task = await store.LoadAsync(command.Id, cancellationToken);
        if (task is null || !command.Actor.CanAccess(task)) return Result.NotFound(TaskMessages.NotFound);
        if (task.Version != command.Version) return Result.Conflict(TaskMessages.Stale);

        var by = await staff.PersonAsync(command.Actor, cancellationToken);
        if (by is null) return Result.Forbidden(TaskMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        if (!task.ChangeStatus(Enum.Parse<ManagerTaskStatus>(command.Status), by, now))
            return Result.Success(task.ToDetail(now));
        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(task.ToDetail(now))
            : Result.Conflict(TaskMessages.Stale);
    }
}
