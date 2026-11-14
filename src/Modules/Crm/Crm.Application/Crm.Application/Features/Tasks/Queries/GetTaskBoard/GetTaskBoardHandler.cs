using Crm.Domain.Tasks;
using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Shared.Mapping;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Tasks;

namespace Crm.Application.Features.Tasks.Queries.GetTaskBoard;

public sealed class GetTaskBoardHandler(ITaskStore store, TimeProvider clock)
    : IQueryHandler<GetTaskBoardQuery, Result<TaskBoardResponse>>
{
    /// <summary>A safety cap; a board with more cards than this is not readable anyway.</summary>
    public const int MaxCards = 500;

    public async ValueTask<Result<TaskBoardResponse>> Handle(GetTaskBoardQuery query,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var (from, to) = TaskBoardPeriods.Range(query.Period, now);
        var tasks = store.Visible(query.Actor, query.AssigneeUserId);
        if (from is { } since) tasks = tasks.Where(x => x.Status != ManagerTaskStatus.Done || x.CompletedAt >= since);
        if (to is { } until) tasks = tasks.Where(x => x.Status != ManagerTaskStatus.Done || x.CompletedAt < until);
        // Newest added on top in every column (round 2, item 11); open cards win the cap over old done ones.
        var rows = await tasks
            .OrderBy(x => x.Status == ManagerTaskStatus.Done)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(MaxCards)
            .ToSummaries(now)
            .ToListAsync(cancellationToken);
        var cards = rows.Select(x => x.Complete(now)).ToArray();
        return Result.Success(new TaskBoardResponse(cards, now));
    }
}
