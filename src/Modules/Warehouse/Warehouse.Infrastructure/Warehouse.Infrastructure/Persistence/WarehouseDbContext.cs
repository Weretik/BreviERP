using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Domain.Stock.Plans;

namespace Warehouse.Infrastructure.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options), IWarehouseDbContext
{
    public const string Schema = "warehouse";

    public DbSet<FabricStock> FabricStocks => Set<FabricStock>();
    public DbSet<FabricMovement> FabricMovements => Set<FabricMovement>();
    public DbSet<FabricMovementRevision> FabricMovementRevisions => Set<FabricMovementRevision>();
    public DbSet<FabricIssue> FabricIssues => Set<FabricIssue>();
    public DbSet<FabricItemPlan> FabricItemPlans => Set<FabricItemPlan>();
    public DbSet<StockOperation> StockOperations => Set<StockOperation>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    public async Task<IWarehouseTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new Transaction(await Database.BeginTransactionAsync(cancellationToken));

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }

    private sealed class Transaction(IDbContextTransaction transaction) : IWarehouseTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
