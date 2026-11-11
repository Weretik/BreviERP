using Warehouse.Domain.Stock;

namespace Warehouse.Application.Abstractions;

/// <summary>Persistence port of the Warehouse module («Склад тканин»); implemented by WarehouseDbContext.</summary>
public interface IWarehouseDbContext
{
    DbSet<FabricStock> FabricStocks { get; }
    DbSet<FabricMovement> FabricMovements { get; }
    DbSet<FabricIssue> FabricIssues { get; }
    DbSet<StockOperation> StockOperations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
