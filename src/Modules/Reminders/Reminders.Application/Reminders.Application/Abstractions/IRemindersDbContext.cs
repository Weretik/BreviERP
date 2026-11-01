using Reminders.Domain.Calendar;
using Reminders.Domain.Chats;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Abstractions;

/// <summary>Persistence port of the Reminders module; implemented by RemindersDbContext.</summary>
public interface IRemindersDbContext
{
    DbSet<ReminderTask> Tasks { get; }
    DbSet<ReminderOccurrence> Occurrences { get; }
    DbSet<ReminderChat> Chats { get; }
    DbSet<SentReminderMessage> SentMessages { get; }
    DbSet<NonWorkingDay> NonWorkingDays { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
