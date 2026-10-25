using Microsoft.EntityFrameworkCore;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace SewingPricing.Jobs.Bootstrap;

public static class PricingBootstrapReferenceVerifier
{
    public static async Task VerifyAsync(ReferenceDbContext db, IReadOnlyList<ReferencePricingInputVersion> versions,
        CancellationToken cancellationToken)
    {
        var fabrics = await db.Fabrics.AsNoTracking().Select(x => new { x.Id, x.Price }).ToDictionaryAsync(x => x.Id, cancellationToken);
        var accessories = await db.GarmentAccessories.AsNoTracking().Select(x => new { x.Id, x.Price }).ToDictionaryAsync(x => x.Id, cancellationToken);
        var operations = await db.GarmentPartOperations.AsNoTracking().Select(x => new { x.Id, x.Min }).ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var version in versions)
        {
            var matches = version.FabricId.HasValue ? fabrics.TryGetValue(version.FabricId.Value, out var fabric) && fabric.Price.Value == version.Value
                : version.AccessoryId.HasValue ? accessories.TryGetValue(version.AccessoryId.Value, out var accessory) && accessory.Price.Value == version.Value
                : !version.OperationId.HasValue || (operations.TryGetValue(version.OperationId.Value, out var operation) && operation.Min == (version.Value ?? 0m));
            if (!matches)
                throw new InvalidDataException($"Reference {version.SourceCode} changed since the frozen catalogue import.");
        }
    }
}

