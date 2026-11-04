using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.GetTasks;

public sealed class GetTasksQueryHandler(ITaskStore store, TimeProvider clock)
    : IQueryHandler<GetTasksQuery, Result<TaskPageResponse>>
{
    public async ValueTask<Result<TaskPageResponse>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var filter = string.IsNullOrEmpty(query.Filter) ? TaskListFilter.Open : Enum.Parse<TaskListFilter>(query.Filter);
        var tasks = Filter(store.Visible(query.Actor, query.AssigneeUserId), filter, now);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var number = TaskNumberSearch.Parse(search);
            tasks = tasks.Where(x => x.Id == number || x.Title.ToLower().Contains(search) ||
                (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)) ||
                x.AssigneeName.ToLower().Contains(search));
        }

        var total = await tasks.CountAsync(cancellationToken);
        var ordered = filter == TaskListFilter.Done
            ? tasks.OrderByDescending(x => x.CompletedAt).ThenByDescending(x => x.Id)
            : tasks.OrderBy(x => x.Status == ManagerTaskStatus.Done)
                .ThenBy(x => x.DueAt >= now)
                .ThenByDescending(x => x.IsUrgent)
                .ThenBy(x => x.DueAt)
                .ThenBy(x => x.Id);
        var rows = await ordered.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToSummaries(now).ToListAsync(cancellationToken);
        return Result.Success(new TaskPageResponse(rows.Select(x => x.Complete(now)).ToArray(), query.Page,
            query.PageSize, total));
    }

    private static IQueryable<ManagerTask> Filter(IQueryable<ManagerTask> tasks, TaskListFilter filter,
        DateTimeOffset now) =>
        filter switch
        {
            TaskListFilter.Overdue => tasks.Where(x => x.Status != ManagerTaskStatus.Done && x.DueAt < now),
            TaskListFilter.Urgent => tasks.Where(x => x.Status != ManagerTaskStatus.Done && x.IsUrgent),
            TaskListFilter.New => tasks.Where(x => x.Status == ManagerTaskStatus.New),
            TaskListFilter.InProgress => tasks.Where(x => x.Status == ManagerTaskStatus.InProgress),
            TaskListFilter.Done => tasks.Where(x => x.Status == ManagerTaskStatus.Done),
            _ => tasks.Where(x => x.Status != ManagerTaskStatus.Done),
        };
}
