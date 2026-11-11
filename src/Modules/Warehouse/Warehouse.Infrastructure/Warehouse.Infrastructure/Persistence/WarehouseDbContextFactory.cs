using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<WarehouseDbContext>()
        .UseNpgsql("Host=localhost;Database=warehouse_design_only;Username=design",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", WarehouseDbContext.Schema)).Options);
}
