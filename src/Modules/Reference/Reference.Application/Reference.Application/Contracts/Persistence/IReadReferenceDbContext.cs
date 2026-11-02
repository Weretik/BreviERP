using Reference.Domain.PpeMarkupRules.Entities;
using Reference.Domain.PricingParameters.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Suppliers.Entities;

namespace Reference.Application.Contracts.Persistence;

public interface IReadReferenceDbContext
{
    DbSet<PricingParameter> PricingParameters { get; }
    DbSet<PpeMarkupRule> PpeMarkupRules { get; }
    DbSet<Fabric> Fabrics { get; }
    DbSet<GarmentAccessory> GarmentAccessories { get; }
    DbSet<GarmentPart> GarmentParts { get; }
    DbSet<GarmentPartOperation> GarmentPartOperations { get; }
    DbSet<Supplier> Suppliers { get; }
}