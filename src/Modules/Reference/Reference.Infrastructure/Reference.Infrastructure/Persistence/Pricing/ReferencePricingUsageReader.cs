using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Pricing;
using Reference.Application.Abstractions;
using Reference.Infrastructure.Persistence;
using Reference.Domain.Fabrics;

namespace Reference.Infrastructure.Persistence.Pricing;

public sealed class ReferencePricingUsageReader(ReferenceDbContext db) : IReferencePricingUsageReader
{
    public Task<bool> IsFabricVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.FabricId == FabricId.From(id), cancellationToken);

    public Task<bool> IsAccessoryVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.AccessoryId == GarmentAccessoryId.From(id), cancellationToken);

    public Task<bool> IsOperationVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.OperationId == GarmentPartOperationId.From(id), cancellationToken);
}
