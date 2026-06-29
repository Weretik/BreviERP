using Reference.Application.Contracts.Persistence;
using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Products.Entities;
using Reference.Domain.Suppliers.Entities;

namespace Reference.Infrastructure.DataBase;

public class ReferenceDbContext(DbContextOptions<ReferenceDbContext> options)
    : DbContext(options), IReadReferenceDbContext
{
    public DbSet<AdditionalReference> AdditionalReferences => Set<AdditionalReference>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();
    public DbSet<GarmentAccessory> GarmentAccessories => Set<GarmentAccessory>();
    public DbSet<GarmentPart> GarmentParts => Set<GarmentPart>();
    public DbSet<GarmentPartOperation> GarmentPartOperations => Set<GarmentPartOperation>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public void DiscardChanges() => ChangeTracker.Clear();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReferenceDbContext).Assembly,
            type => type.Namespace?.StartsWith("Reference.Infrastructure") ?? false);
    }
}
