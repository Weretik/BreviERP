using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ai.Infrastructure.Persistence;

public sealed class AiDbContextFactory : IDesignTimeDbContextFactory<AiDbContext>
{
    public AiDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AiDbContext>()
        .UseNpgsql("Host=localhost;Database=ai_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", AiDbContext.Schema)).Options);
}
