using BuildingBlocks.Domain.Identifiers.ReferenceData;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Pricing;
using Reference.Application.Abstractions;
using Reference.Infrastructure.Persistence;
using Reference.Domain.Fabrics;
using Reference.Application.Abstractions.Pricing;


namespace Reference.Infrastructure.Persistence.Repositories.Pricing;

public sealed class ReferencePricingUsageReader(ReferenceDbContext db) : IReferencePricingUsageReader
{
    public Task<bool> IsFabricVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.FabricId == new FabricId(id), cancellationToken);

    public Task<bool> IsAccessoryVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.AccessoryId == new GarmentAccessoryId(id), cancellationToken);

    public Task<bool> IsOperationVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.OperationId == new GarmentPartOperationId(id), cancellationToken);
}
