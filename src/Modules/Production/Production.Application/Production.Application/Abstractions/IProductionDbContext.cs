using Production.Domain.FabricNorms;
using Production.Domain.Orders;
using Production.Domain.Settings;
using Production.Domain.Staff;

namespace Production.Application.Abstractions;

/// <summary>Persistence port of the Production module («Швейний цех»); implemented by ProductionDbContext.</summary>
public interface IProductionDbContext
{
    DbSet<WorkshopEmployee> Employees { get; }
    DbSet<ModelFabricNorm> FabricNorms { get; }
    DbSet<ProductionOrder> Orders { get; }
    DbSet<ProductionOrderChange> OrderChanges { get; }
    DbSet<ProductionOrderFile> OrderFiles { get; }
    DbSet<ProductionTelegramDelivery> TelegramDeliveries { get; }
    DbSet<WorkshopSettings> Settings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
