using Reminders.Application.Abstractions;
using Reminders.Application.Features.Calendar.Shared;
using Reminders.Application.Features.Tasks.Shared;
using Reminders.Application.Features.Tasks.Shared.Mapping;

namespace Reminders.Application.Features.Tasks.Queries.PreviewReminderDueDates;

public sealed class PreviewReminderDueDatesHandler(IRemindersDbContext db, ReminderClock clock)
    : IQueryHandler<PreviewReminderDueDatesQuery, Result<IReadOnlyList<DueDatePreview>>>
{
    public async ValueTask<Result<IReadOnlyList<DueDatePreview>>> Handle(PreviewReminderDueDatesQuery query, CancellationToken cancellationToken)
    {
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        return Result.Success(ReminderTaskMapper.Preview(ReminderTaskMapper.ToSchedule(query.Schedule), query.From ?? clock.Today, calendar));
    }
}
