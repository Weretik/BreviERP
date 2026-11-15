using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Tasks;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options), ISchedulingDbContext
{
    public const string Schema = "scheduling";

    /// <summary>The advisory lock of the scheduler tick (database rules 4).</summary>
    public const long TickLockKey = 681_025_101;

    public DbSet<ScheduledTask> Tasks => Set<ScheduledTask>();
    public DbSet<ScheduledTaskRun> Runs => Set<ScheduledTaskRun>();

    /// <summary>
    /// A session advisory lock on the context's connection, kept open until the lock is disposed: the tick saves
    /// several times and starts the job between saves, so a transaction lock would hold its claims unseen.
    /// </summary>
    public async Task<IAsyncDisposable?> TryLockTickAsync(CancellationToken cancellationToken)
    {
        if (!Database.IsNpgsql()) return new TickLock(this, false); // SQLite unit tests run one tick at a time.
        await Database.OpenConnectionAsync(cancellationToken);
        var locked = await Database.SqlQuery<bool>($"SELECT pg_try_advisory_lock({TickLockKey}) AS \"Value\"")
            .SingleAsync(cancellationToken);
        if (locked) return new TickLock(this, true);
        await Database.CloseConnectionAsync();
        return null;
    }

    private sealed class TickLock(SchedulingDbContext db, bool held) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            if (!held) return;
            try
            {
                await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_unlock({TickLockKey})");
            }
            finally
            {
                // Closing the connection releases a session lock anyway; pooled connections reset it.
                await db.Database.CloseConnectionAsync();
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);
    }
}
