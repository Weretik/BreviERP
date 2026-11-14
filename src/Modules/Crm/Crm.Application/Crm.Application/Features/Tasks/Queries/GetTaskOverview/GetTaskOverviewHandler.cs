using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Tasks.Enums;

namespace Crm.Application.Features.Tasks.Queries.GetTaskOverview;

public sealed class GetTaskOverviewHandler(ITaskStore store, ITaskStaffReader staff, TimeProvider clock)
    : IQueryHandler<GetTaskOverviewQuery, Result<TaskOverviewResponse>>
{
    public async ValueTask<Result<TaskOverviewResponse>> Handle(GetTaskOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);

        var counts = await store.Visible(query.Actor, query.AssigneeUserId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Open = g.Count(x => x.Status != ManagerTaskStatus.Done),
                Overdue = g.Count(x => x.Status != ManagerTaskStatus.Done && x.DueAt < now),
                Urgent = g.Count(x => x.Status != ManagerTaskStatus.Done && x.IsUrgent),
                New = g.Count(x => x.Status == ManagerTaskStatus.New),
                InProgress = g.Count(x => x.Status == ManagerTaskStatus.InProgress),
                Done = g.Count(x => x.Status == ManagerTaskStatus.Done),
                DoneLastWeek = g.Count(x => x.Status == ManagerTaskStatus.Done && x.CompletedAt >= weekAgo),
                DoneLastMonth = g.Count(x => x.Status == ManagerTaskStatus.Done && x.CompletedAt >= monthAgo),
                OnTimeLastMonth = g.Count(x => x.Status == ManagerTaskStatus.Done && x.CompletedAt >= monthAgo &&
                    x.CompletedAt <= x.DueAt),
            })
            .SingleOrDefaultAsync(cancellationToken);

        var load = await LoadAsync(query, now, cancellationToken);
        int? onTime = counts is { DoneLastMonth: > 0 }
            ? (int)Math.Round(100.0 * counts.OnTimeLastMonth / counts.DoneLastMonth)
            : null;
        return Result.Success(counts is null
            ? new TaskOverviewResponse(0, 0, 0, 0, 0, 0, 0, null, load)
            : new TaskOverviewResponse(counts.Open, counts.Overdue, counts.Urgent, counts.New, counts.InProgress,
                counts.Done, counts.DoneLastWeek, onTime, load));
    }

    /// <summary>
    /// Every active manager for Director/Admin (with zeros), the Manager alone otherwise. Open counts come with
    /// done in 7 days and on-time share of the last 30 days, the «Навантаження» tab (TSK-001 R-11).
    /// </summary>
    private async Task<IReadOnlyList<TaskLoadResponse>> LoadAsync(GetTaskOverviewQuery query, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);
        var rows = await store.Visible(query.Actor, null)
            .Where(x => x.Status != ManagerTaskStatus.Done || x.CompletedAt >= monthAgo)
            .GroupBy(x => new { x.AssigneeUserId, x.AssigneeName })
            .Select(g => new
            {
                g.Key.AssigneeUserId,
                g.Key.AssigneeName,
                Open = g.Count(x => x.Status != ManagerTaskStatus.Done),
                Overdue = g.Count(x => x.Status != ManagerTaskStatus.Done && x.DueAt < now),
                Urgent = g.Count(x => x.Status != ManagerTaskStatus.Done && x.IsUrgent),
                InProgress = g.Count(x => x.Status == ManagerTaskStatus.InProgress),
                DoneLastWeek = g.Count(x => x.Status == ManagerTaskStatus.Done && x.CompletedAt >= weekAgo),
                DoneLastMonth = g.Count(x => x.Status == ManagerTaskStatus.Done),
                OnTimeLastMonth = g.Count(x => x.Status == ManagerTaskStatus.Done && x.CompletedAt <= x.DueAt),
            })
            .ToListAsync(cancellationToken);
        var byUser = rows.GroupBy(x => x.AssigneeUserId).ToDictionary(g => g.Key, g =>
        {
            var doneLastMonth = g.Sum(x => x.DoneLastMonth);
            int? onTime = doneLastMonth > 0
                ? (int)Math.Round(100.0 * g.Sum(x => x.OnTimeLastMonth) / doneLastMonth)
                : null;
            return new TaskLoadResponse(g.Key, g.First().AssigneeName, g.Sum(x => x.Open), g.Sum(x => x.Overdue),
                g.Sum(x => x.Urgent), g.Sum(x => x.InProgress), g.Sum(x => x.DoneLastWeek), onTime);
        });

        if (!query.Actor.IsAdmin)
            return byUser.TryGetValue(query.Actor.UserId, out var own) ? [own] : [];

        var managers = await staff.ListAssigneesAsync(cancellationToken);
        var result = managers.Select(m => byUser.TryGetValue(m.UserId, out var row)
            ? row with { Name = m.Name }
            : new TaskLoadResponse(m.UserId, m.Name, 0, 0, 0, 0, 0, null)).ToList();
        // Tasks of a blocked or demoted manager stay visible until they are given to someone else.
        result.AddRange(byUser.Values.Where(x => managers.All(m => m.UserId != x.UserId)));
        return result;
    }
}
