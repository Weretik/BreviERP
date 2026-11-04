using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Building;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Application.Tasks.Staff;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.CreateTask;

public sealed class CreateTaskCommandHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
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
