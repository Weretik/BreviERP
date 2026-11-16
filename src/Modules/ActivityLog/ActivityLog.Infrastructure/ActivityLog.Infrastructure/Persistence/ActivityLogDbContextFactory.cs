using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ActivityLog.Infrastructure.Persistence;

public sealed class ActivityLogDbContextFactory : IDesignTimeDbContextFactory<ActivityLogDbContext>
{
    public ActivityLogDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ActivityLogDbContext>()
        .UseNpgsql("Host=localhost;Database=activity_log_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ActivityLogDbContext.Schema)).Options);
}
