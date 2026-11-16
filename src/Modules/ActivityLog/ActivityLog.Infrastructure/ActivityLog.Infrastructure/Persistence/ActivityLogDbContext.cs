using ActivityLog.Application.Abstractions;
using ActivityLog.Domain.Entries;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ActivityLog.Infrastructure.Persistence;

public sealed class ActivityLogDbContext(DbContextOptions<ActivityLogDbContext> options)
    : DbContext(options), IActivityLogDbContext
{
    public const string Schema = "activity_log";

    public DbSet<ActivityEntry> Entries => Set<ActivityEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.AddTrigramSearch();
        builder.ApplyConfigurationsFromAssembly(typeof(ActivityLogDbContext).Assembly);
    }
}
