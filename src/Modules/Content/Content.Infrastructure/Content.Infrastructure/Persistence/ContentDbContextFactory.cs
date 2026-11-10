using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Content.Infrastructure.Persistence;

public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ContentDbContext>()
        .UseNpgsql("Host=localhost;Database=content_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ContentDbContext.Schema)).Options);
}
