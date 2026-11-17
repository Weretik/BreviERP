using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Tasks.Enums;

namespace Crm.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksHandler(ITaskStore store, TimeProvider clock)
    : IQueryHandler<GetTasksQuery, Result<TaskPageResponse>>
{
    public async ValueTask<Result<TaskPageResponse>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var filter = string.IsNullOrEmpty(query.Filter) ? TaskListFilter.Open : Enum.Parse<TaskListFilter>(query.Filter);
        var tasks = Filter(store.Visible(query.Actor, query.AssigneeUserId), filter, now);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var number = TaskNumberSearch.Parse(query.Search.Trim().ToLower());
            tasks = tasks.WhereEveryWord(query.Search, pattern => x => x.Id == new ManagerTaskId(number) || EF.Functions.Like(EF.Property<string>(x, SearchWords.Column), pattern, SearchWords.Escape));
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
