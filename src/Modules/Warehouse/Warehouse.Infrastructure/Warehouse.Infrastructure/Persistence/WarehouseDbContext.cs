using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options), IWarehouseDbContext
{
    public const string Schema = "warehouse";

    public DbSet<FabricStock> FabricStocks => Set<FabricStock>();
    public DbSet<FabricMovement> FabricMovements => Set<FabricMovement>();
    public DbSet<FabricIssue> FabricIssues => Set<FabricIssue>();
    public DbSet<StockOperation> StockOperations => Set<StockOperation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
