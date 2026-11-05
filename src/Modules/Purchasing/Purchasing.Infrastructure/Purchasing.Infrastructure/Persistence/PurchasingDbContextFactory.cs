using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Purchasing.Infrastructure.Persistence;

public sealed class PurchasingDbContextFactory : IDesignTimeDbContextFactory<PurchasingDbContext>
{
    public PurchasingDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<PurchasingDbContext>()
        .UseNpgsql("Host=localhost;Database=purchasing_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", PurchasingDbContext.Schema)).Options);
}
