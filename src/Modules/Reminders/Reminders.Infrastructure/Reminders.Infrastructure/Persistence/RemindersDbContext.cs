using Microsoft.EntityFrameworkCore;
using Reminders.Application.Abstractions;
using Reminders.Domain.Calendar;
using Reminders.Domain.Chats;
using Reminders.Domain.Dispatching;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Tasks;

namespace Reminders.Infrastructure.Persistence;

public sealed class RemindersDbContext(DbContextOptions<RemindersDbContext> options) : DbContext(options), IRemindersDbContext
{
    public const string Schema = "reminders";

    public DbSet<ReminderTask> Tasks => Set<ReminderTask>();
    public DbSet<ReminderOccurrence> Occurrences => Set<ReminderOccurrence>();
    public DbSet<ReminderChat> Chats => Set<ReminderChat>();
    public DbSet<SentReminderMessage> SentMessages => Set<SentReminderMessage>();
    public DbSet<NonWorkingDay> NonWorkingDays => Set<NonWorkingDay>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(RemindersDbContext).Assembly);
    }
}
