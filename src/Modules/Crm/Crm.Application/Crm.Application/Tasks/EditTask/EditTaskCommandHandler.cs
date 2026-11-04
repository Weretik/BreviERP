using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Building;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Application.Tasks.Staff;

namespace Crm.Application.Tasks.EditTask;

public sealed class EditTaskCommandHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<EditTaskCommand, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(EditTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden(TaskMessages.AdminOnly);
        var task = await store.LoadAsync(command.Id, cancellationToken);
        if (task is null) return Result.NotFound(TaskMessages.NotFound);
        if (task.Version != command.Input.Version) return Result.Conflict(TaskMessages.Stale);

        var by = await staff.PersonAsync(command.Actor, cancellationToken);
        if (by is null) return Result.Forbidden(TaskMessages.ActorUnknown);
        var draft = await staff.ResolveAsync(store, command.Input, cancellationToken);
        if (!draft.IsSuccess) return draft.Map(_ => default(TaskDetailResponse)!);

        var now = clock.GetUtcNow();
        task.Edit(draft.Value, by, now);
        return await store.TrySaveAsync(cancellationToken)
            ? Result.Success(task.ToDetail(now))
            : Result.Conflict(TaskMessages.Stale);
    }
}
