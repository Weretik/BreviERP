namespace ActivityLog.Application.Abstractions;

/// <summary>Persistence port of «Журнал дій»; implemented by ActivityLogDbContext.</summary>
public interface IActivityLogDbContext
{
    DbSet<ActivityEntry> Entries { get; }
    DbSet<ActivityActor> Actors { get; }
    DbSet<ActivitySubjectType> SubjectTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
