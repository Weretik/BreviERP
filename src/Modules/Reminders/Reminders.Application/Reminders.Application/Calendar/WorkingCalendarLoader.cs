using Reminders.Application.Abstractions;
using Reminders.Domain.Calendar;

namespace Reminders.Application.Calendar;

public static class WorkingCalendarLoader
{
    public static async Task<WorkingCalendar> LoadCalendarAsync(this IRemindersDbContext db, CancellationToken cancellationToken) =>
        new(await db.NonWorkingDays.AsNoTracking().Select(x => x.Date).ToListAsync(cancellationToken));
}
