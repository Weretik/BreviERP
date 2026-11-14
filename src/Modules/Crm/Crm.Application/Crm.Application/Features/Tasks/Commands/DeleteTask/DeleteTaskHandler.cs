using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskHandler(ITaskStore store) : ICommandHandler<DeleteTaskCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.IsAdmin) return Result.Forbidden(TaskMessages.AdminOnly);
        var task = await store.LoadAsync(command.Id, cancellationToken);
        if (task is null) return Result.NotFound(TaskMessages.NotFound);
        if (task.Version != command.Version) return Result.Conflict(TaskMessages.Stale);

        store.ManagerTasks.Remove(task);
        return await store.TrySaveAsync(cancellationToken) ? Result.NoContent() : Result.Conflict(TaskMessages.Stale);
    }
}
