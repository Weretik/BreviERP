using Microsoft.EntityFrameworkCore.Design;

namespace Production.Infrastructure.Persistence;

public sealed class ProductionDbContextFactory : IDesignTimeDbContextFactory<ProductionDbContext>
{
    public ProductionDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ProductionDbContext>()
        .UseNpgsql("Host=localhost;Database=production_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ProductionDbContext.Schema)).Options);
}
