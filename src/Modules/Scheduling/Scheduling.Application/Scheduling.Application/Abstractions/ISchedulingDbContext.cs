namespace Scheduling.Application.Abstractions;

/// <summary>Persistence port of the Scheduling module; implemented by SchedulingDbContext.</summary>
public interface ISchedulingDbContext
{
    DbSet<ScheduledTask> Tasks { get; }
    DbSet<ScheduledTaskRun> Runs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Holds the tick lock until disposed; null when another tick holds it (a Cloud Scheduler retry or a slow tick).
    /// </summary>
    Task<IAsyncDisposable?> TryLockTickAsync(CancellationToken cancellationToken);
}
