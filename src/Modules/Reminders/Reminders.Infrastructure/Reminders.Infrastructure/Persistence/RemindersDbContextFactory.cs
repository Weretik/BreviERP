using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Reminders.Infrastructure.Persistence;

public sealed class RemindersDbContextFactory : IDesignTimeDbContextFactory<RemindersDbContext>
{
    public RemindersDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<RemindersDbContext>()
        .UseNpgsql("Host=localhost;Database=reminders_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", RemindersDbContext.Schema)).Options);
}
