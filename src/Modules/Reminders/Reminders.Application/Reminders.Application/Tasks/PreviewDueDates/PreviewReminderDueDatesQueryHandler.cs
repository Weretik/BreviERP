using Reminders.Application.Abstractions;
using Reminders.Application.Calendar;
using Reminders.Application.Contracts;
using Reminders.Application.Tasks.Mapping;

namespace Reminders.Application.Tasks.PreviewDueDates;

public sealed class PreviewReminderDueDatesQueryHandler(IRemindersDbContext db, ReminderClock clock)
    : IQueryHandler<PreviewReminderDueDatesQuery, Result<IReadOnlyList<DueDatePreview>>>
{
    public async ValueTask<Result<IReadOnlyList<DueDatePreview>>> Handle(PreviewReminderDueDatesQuery query, CancellationToken cancellationToken)
    {
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        return Result.Success(ReminderTaskMapper.Preview(ReminderTaskMapper.ToSchedule(query.Schedule), query.From ?? clock.Today, calendar));
    }
}
