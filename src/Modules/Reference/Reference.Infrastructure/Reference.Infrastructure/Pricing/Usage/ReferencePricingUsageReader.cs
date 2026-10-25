using Reference.Application.Contracts.Pricing;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Pricing.Reading;

public sealed class ReferencePricingUsageReader(ReferenceDbContext db) : IReferencePricingUsageReader
{
    public Task<bool> IsFabricVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.FabricId == FabricId.From(id), cancellationToken);

    public Task<bool> IsAccessoryVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.AccessoryId == GarmentAccessoryId.From(id), cancellationToken);

    public Task<bool> IsOperationVersionedAsync(int id, CancellationToken cancellationToken) =>
        db.Set<ReferencePricingInputVersion>().AnyAsync(x => x.OperationId == GarmentPartOperationId.From(id), cancellationToken);
}
