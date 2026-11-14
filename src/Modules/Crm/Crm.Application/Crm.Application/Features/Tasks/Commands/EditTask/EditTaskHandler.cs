using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Building;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.EditTask;

public sealed class EditTaskHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
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
