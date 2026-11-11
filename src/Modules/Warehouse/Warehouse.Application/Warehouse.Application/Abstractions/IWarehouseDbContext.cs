using Warehouse.Domain.Stock;

namespace Warehouse.Application.Abstractions;

/// <summary>Persistence port of the Warehouse module («Склад тканин»); implemented by WarehouseDbContext.</summary>
public interface IWarehouseDbContext
{
    DbSet<FabricStock> FabricStocks { get; }
    DbSet<FabricMovement> FabricMovements { get; }
    DbSet<FabricIssue> FabricIssues { get; }
    DbSet<FabricItemPlan> FabricItemPlans { get; }
    DbSet<StockOperation> StockOperations { get; }
    DbSet<FabricMovementRevision> FabricMovementRevisions { get; }
    DbSet<StockTransfer> StockTransfers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Several saves that stand or fall together (the transfer replaces the whole stock).</summary>
    Task<IWarehouseTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}

/// <summary>Rolled back when disposed without <see cref="CommitAsync"/>.</summary>
public interface IWarehouseTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
