using BuildingBlocks.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reminders.Domain;
using Reminders.Domain.Tasks;
using Reminders.Infrastructure.Persistence;

namespace Reminders.Infrastructure.Persistence.Seeding;

/// <summary>
/// Adds the default tasks once, into an empty table, without a chat: nothing is sent until Admin assigns
/// approved chats. Archived tasks keep their rows, so a later deploy never recreates deleted tasks.
/// </summary>
public sealed class DefaultReminderTaskSeeder(RemindersDbContext db, TimeProvider time, ILogger<DefaultReminderTaskSeeder> logger) : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (await db.Tasks.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped reminder task seeding because tasks already exist.");
            return;
        }
        var now = time.GetUtcNow();
        var today = ReminderTime.TodayInKyiv(now);
        foreach (var (title, organization, schedule) in DefaultReminderTasks.All)
            db.Tasks.Add(ReminderTask.Create(title, organization, schedule, null, today, true, now).Value);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded reminder tasks. Added: {Count}", DefaultReminderTasks.All.Count);
    }
}
