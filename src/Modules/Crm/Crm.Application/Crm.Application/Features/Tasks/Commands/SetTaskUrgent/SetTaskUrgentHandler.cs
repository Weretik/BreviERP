using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Building;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.SetTaskUrgent;

public sealed class SetTaskUrgentHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
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
