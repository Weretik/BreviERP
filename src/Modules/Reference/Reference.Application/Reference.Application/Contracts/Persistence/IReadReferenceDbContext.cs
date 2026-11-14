using Reference.Domain.PpeMarkupRules;
using Reference.Domain.PricingParameters;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

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