using ActivityLog.Application.Abstractions;
using ActivityLog.Domain.Entries;
using ActivityLog.Domain.Filters;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.Infrastructure.Persistence;

public sealed class ActivityLogDbContext(DbContextOptions<ActivityLogDbContext> options)
    : DbContext(options), IActivityLogDbContext
{
    public const string Schema = "activity_log";

    public DbSet<ActivityEntry> Entries => Set<ActivityEntry>();
    public DbSet<ActivityActor> Actors => Set<ActivityActor>();
    public DbSet<ActivitySubjectType> SubjectTypes => Set<ActivitySubjectType>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.AddTypedIds(typeof(ActivityEntry).Assembly);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.AddTrigramSearch();
        builder.ApplyConfigurationsFromAssembly(typeof(ActivityLogDbContext).Assembly);
    }
}
