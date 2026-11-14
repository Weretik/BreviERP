using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Building;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : ICommandHandler<CreateTaskCommand, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden(TaskMessages.AdminOnly);
        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(TaskMessages.ActorUnknown);

        var draft = await staff.ResolveAsync(store, command.Input, cancellationToken);
        if (!draft.IsSuccess) return draft.Map(_ => default(TaskDetailResponse)!);

        var now = clock.GetUtcNow();
        var task = new ManagerTask(draft.Value, author, now);
        store.ManagerTasks.Add(task);
        await store.SaveChangesAsync(cancellationToken);
        return Result.Created(task.ToDetail(now));
    }
}
