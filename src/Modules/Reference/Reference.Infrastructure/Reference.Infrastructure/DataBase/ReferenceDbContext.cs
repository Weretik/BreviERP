using BuildingBlocks.Infrastructure.Outbox;
using Reference.Application.Contracts.Persistence;
using Reference.Domain.FabricColors;
using Reference.Domain.PpeMarkupRules;
using Reference.Domain.PricingParameters;
using Reference.Domain.ProductSections;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Infrastructure.DataBase;

public partial class ReferenceDbContext(DbContextOptions<ReferenceDbContext> options)
    : DbContext(options), IReadReferenceDbContext
{
    public DbSet<PricingParameter> PricingParameters => Set<PricingParameter>();
    public DbSet<PpeMarkupRule> PpeMarkupRules => Set<PpeMarkupRule>();
    public DbSet<ProductSection> ProductSections => Set<ProductSection>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();
    public DbSet<GarmentAccessory> GarmentAccessories => Set<GarmentAccessory>();
    public DbSet<GarmentPart> GarmentParts => Set<GarmentPart>();
    public DbSet<GarmentPartOperation> GarmentPartOperations => Set<GarmentPartOperation>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public void DiscardChanges() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("reference");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReferenceDbContext).Assembly,
            type => type.Namespace?.StartsWith("Reference.Infrastructure") ?? false);
        modelBuilder.AddOutbox();
    }
}
