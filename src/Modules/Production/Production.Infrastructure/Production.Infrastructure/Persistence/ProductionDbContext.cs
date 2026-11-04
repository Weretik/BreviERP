using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Domain.Orders;
using Production.Domain.Settings;
using Production.Domain.Staff;

namespace Production.Infrastructure.Persistence;

/// <summary>Database of the sewing workshop («Швейний цех»): its own schema <c>production</c> and migrations history.</summary>
public sealed class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options), IProductionDbContext
{
    public const string Schema = "production";

    public DbSet<WorkshopEmployee> Employees => Set<WorkshopEmployee>();
    public DbSet<ModelFabricNorm> FabricNorms => Set<ModelFabricNorm>();
    public DbSet<ProductionOrder> Orders => Set<ProductionOrder>();
    public DbSet<ProductionOrderChange> OrderChanges => Set<ProductionOrderChange>();
    public DbSet<ProductionOrderFile> OrderFiles => Set<ProductionOrderFile>();
    public DbSet<ProductionTelegramDelivery> TelegramDeliveries => Set<ProductionTelegramDelivery>();
    public DbSet<WorkshopSettings> Settings => Set<WorkshopSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(ProductionDbContext).Assembly);
    }
}
