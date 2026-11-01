using Reminders.Application.Abstractions;
using Reminders.Application.Calendar;
using Reminders.Application.Contracts;
using Reminders.Application.Tasks.Mapping;

namespace Reminders.Application.Tasks.GetTasks;

public sealed class GetReminderTasksQueryHandler(IRemindersDbContext db, ReminderClock clock)
    : IQueryHandler<GetReminderTasksQuery, Result<IReadOnlyList<ReminderTaskResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ReminderTaskResponse>>> Handle(GetReminderTasksQuery query, CancellationToken cancellationToken)
    {
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        var tasks = await db.Tasks.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var chats = await db.Chats.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
        var today = clock.Today;
        return Result.Success<IReadOnlyList<ReminderTaskResponse>>(tasks
            .Select(x => ReminderTaskMapper.ToResponse(x, x.ChatId is { } id ? chats.GetValueOrDefault(id) : null, today, calendar))
            .OrderBy(x => x.Organization == "none").ThenBy(x => x.Organization).ThenBy(x => x.Title, StringComparer.CurrentCulture)
            .ToArray());
    }
}
