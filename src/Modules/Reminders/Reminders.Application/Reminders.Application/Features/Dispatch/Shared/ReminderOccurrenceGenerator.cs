using Reminders.Application.Abstractions;
using Reminders.Domain.Calendar;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Features.Dispatch.Shared;

/// <summary>Creates the occurrences whose reminder window has started. One occurrence per task and period.</summary>
public sealed class ReminderOccurrenceGenerator(IRemindersDbContext db)
{
    public async Task<int> EnsureAsync(IReadOnlyCollection<ReminderTask> tasks, DateOnly today, WorkingCalendar calendar,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var taskIds = tasks.Select(x => x.Id).ToArray();
        var existing = (await db.Occurrences.AsNoTracking().Where(x => taskIds.Contains(x.TaskId))
                .Select(x => new { x.TaskId, x.PeriodStart }).ToListAsync(cancellationToken))
            .Select(x => (x.TaskId, x.PeriodStart)).ToHashSet();
        var created = 0;
        foreach (var task in tasks)
        {
            var to = today.AddDays(task.Schedule.LeadDays);
            if (to < task.ActiveFrom) continue;
            foreach (var (period, dueDate) in task.Schedule.PeriodsDueBetween(task.ActiveFrom, to, calendar))
            {
                if (!existing.Add((task.Id, period.Start))) continue;
                db.Occurrences.Add(ReminderOccurrence.Open(task.Id, period, dueDate, now));
                created++;
            }
        }
        if (created > 0) await db.SaveChangesAsync(cancellationToken);
        return created;
    }
}
