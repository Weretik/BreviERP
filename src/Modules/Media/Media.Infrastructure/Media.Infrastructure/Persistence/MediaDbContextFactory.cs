using Microsoft.EntityFrameworkCore.Design;

namespace Media.Infrastructure.Persistence;

public sealed class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<MediaDbContext>()
        .UseNpgsql("Host=localhost;Database=media_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", MediaDbContext.Schema)).Options);
}
