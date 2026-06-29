using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Products.Entities;
using Reference.Domain.Suppliers.Entities;

namespace Reference.Application.Contracts.Persistence;

public interface IReadReferenceDbContext
{
    DbSet<AdditionalReference> AdditionalReferences { get; }
    DbSet<Fabric> Fabrics { get; }
    DbSet<GarmentAccessory> GarmentAccessories { get; }
    DbSet<GarmentPart> GarmentParts { get; }
    DbSet<GarmentPartOperation> GarmentPartOperations { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Supplier> Suppliers { get; }
}
