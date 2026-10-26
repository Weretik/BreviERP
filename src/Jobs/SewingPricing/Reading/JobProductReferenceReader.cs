using Catalog.Application.Contracts.Reference;
using Microsoft.EntityFrameworkCore;
using Reference.Infrastructure.DataBase;

namespace SewingPricing.Jobs.Reading;

public sealed class JobProductReferenceReader(ReferenceDbContext db) : IProductReferenceReader
{
    public async Task<ProductReferenceData> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var suppliers = await db.Suppliers.AsNoTracking().Select(x => new ReferenceItem(x.Id.Value, x.Name)).ToListAsync(cancellationToken);
        var fabrics = await db.Fabrics.AsNoTracking().Select(x => new PricedReferenceItem(x.Id.Value, x.Name, x.Price.Value)).ToListAsync(cancellationToken);
        var accessories = await db.GarmentAccessories.AsNoTracking().Select(x => new PricedReferenceItem(x.Id.Value, x.Name, x.Price.Value)).ToListAsync(cancellationToken);
        var operations = await db.GarmentPartOperations.AsNoTracking().Select(x => new OperationReferenceItem(x.Id.Value, x.Name, x.Min)).ToListAsync(cancellationToken);
        var additional = await db.AdditionalReferences.AsNoTracking().Select(x => new AdditionalReferenceItem(x.Id.Value, x.Name, x.Key, x.Value, x.Unit)).ToListAsync(cancellationToken);
        return new(suppliers.ToDictionary(x => x.Id), fabrics.ToDictionary(x => x.Id), accessories.ToDictionary(x => x.Id),
            operations.ToDictionary(x => x.Id), additional.ToDictionary(x => x.Id));
    }
}
