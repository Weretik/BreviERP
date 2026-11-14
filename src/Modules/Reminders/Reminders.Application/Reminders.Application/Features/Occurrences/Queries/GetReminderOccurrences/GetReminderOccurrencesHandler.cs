using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Application.Features.Occurrences.Queries.GetReminderOccurrences;

public sealed class GetReminderOccurrencesHandler(IRemindersDbContext db)
    : IQueryHandler<GetReminderOccurrencesQuery, Result<ReminderOccurrencePage>>
{
    public async ValueTask<Result<ReminderOccurrencePage>> Handle(GetReminderOccurrencesQuery query, CancellationToken cancellationToken)
    {
        var source = db.Occurrences.AsNoTracking();
        if (query.Status is not null)
        {
            var status = query.Status == "done" ? ReminderOccurrenceStatus.Done : ReminderOccurrenceStatus.Open;
            source = source.Where(x => x.Status == status);
        }
        if (query.TaskId is { } taskId) source = source.Where(x => x.TaskId == new ReminderTaskId(taskId));

        var total = await source.CountAsync(cancellationToken);
        var items = await source.Include(x => x.Actions)
            .OrderByDescending(x => x.DueDate).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var taskIds = items.Select(x => x.TaskId).Distinct().ToArray();
        var tasks = await db.Tasks.AsNoTracking().Where(x => taskIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        return Result.Success(new ReminderOccurrencePage(
            items.Select(x => ReminderOccurrenceMapper.ToResponse(x, tasks[x.TaskId])).ToArray(), total));
    }
}
