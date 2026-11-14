using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    public SchedulingDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<SchedulingDbContext>()
        .UseNpgsql("Host=localhost;Database=scheduling_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", SchedulingDbContext.Schema)).Options);
}
