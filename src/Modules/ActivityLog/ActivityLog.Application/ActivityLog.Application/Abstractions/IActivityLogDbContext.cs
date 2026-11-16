namespace ActivityLog.Application.Abstractions;

/// <summary>Persistence port of «Журнал дій»; implemented by ActivityLogDbContext.</summary>
public interface IActivityLogDbContext
{
    DbSet<ActivityEntry> Entries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
