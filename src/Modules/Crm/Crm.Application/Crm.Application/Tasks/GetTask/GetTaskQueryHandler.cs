using Crm.Application.Tasks.Access;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;

namespace Crm.Application.Tasks.GetTask;

public sealed class GetTaskQueryHandler(ITaskStore store, TimeProvider clock)
    : IQueryHandler<GetTaskQuery, Result<TaskDetailResponse>>
{
    public async ValueTask<Result<TaskDetailResponse>> Handle(GetTaskQuery query, CancellationToken cancellationToken)
    {
        var task = await store.ManagerTasks.AsNoTracking()
            .Include(x => x.Steps).Include(x => x.Events).AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        return task is null || !query.Actor.CanAccess(task)
            ? Result.NotFound(TaskMessages.NotFound)
            : Result.Success(task.ToDetail(clock.GetUtcNow()));
    }
}
