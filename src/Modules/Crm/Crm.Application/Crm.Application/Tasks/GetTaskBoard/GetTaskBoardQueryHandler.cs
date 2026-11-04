using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.Mapping;
using Crm.Application.Tasks.Persistence;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.GetTaskBoard;

public sealed class GetTaskBoardQueryHandler(ITaskStore store, TimeProvider clock)
    : IQueryHandler<GetTaskBoardQuery, Result<TaskBoardResponse>>
{
    /// <summary>How long a done task stays in «Виконано».</summary>
    public static readonly TimeSpan DoneWindow = TimeSpan.FromDays(7);

    /// <summary>A safety cap; a board with more cards than this is not readable anyway.</summary>
    public const int MaxCards = 500;

    public async ValueTask<Result<TaskBoardResponse>> Handle(GetTaskBoardQuery query,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var doneSince = now - DoneWindow;
        var rows = await store.Visible(query.Actor, query.AssigneeUserId)
            .Where(x => x.Status != ManagerTaskStatus.Done || x.CompletedAt >= doneSince)
            .OrderBy(x => x.Status == ManagerTaskStatus.Done)
            .ThenByDescending(x => x.IsUrgent)
            .ThenBy(x => x.DueAt)
            .ThenBy(x => x.Id)
            .Take(MaxCards)
            .ToSummaries(now)
            .ToListAsync(cancellationToken);
        var cards = rows.Select(x => x.Complete(now)).ToArray();
        var open = cards.Where(x => x.Status != nameof(ManagerTaskStatus.Done));
        var done = cards.Where(x => x.Status == nameof(ManagerTaskStatus.Done)).OrderByDescending(x => x.CompletedAt);
        return Result.Success(new TaskBoardResponse(open.Concat(done).ToArray(), now));
    }
}
