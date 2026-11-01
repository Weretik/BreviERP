using Reminders.Domain;

namespace Reminders.Application.Abstractions;

/// <summary>Current instant and the current date in Kyiv.</summary>
public sealed class ReminderClock(TimeProvider time)
{
    public DateTimeOffset Now => time.GetUtcNow();
    public DateOnly Today => ReminderTime.TodayInKyiv(Now);
}
