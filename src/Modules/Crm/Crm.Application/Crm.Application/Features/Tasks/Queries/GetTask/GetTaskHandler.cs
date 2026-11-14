using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Access;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Queries.GetTask;

public sealed class GetTaskHandler(ITaskStore store, TimeProvider clock)
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
