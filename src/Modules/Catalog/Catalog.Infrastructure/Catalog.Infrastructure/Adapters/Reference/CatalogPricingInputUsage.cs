using BuildingBlocks.Domain.Identifiers.ReferenceData;
using Reference.Contracts.Pricing;
using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Catalog.Infrastructure.Persistence;
using Catalog.Domain.Products.Sewing.Enums;
using Catalog.Domain.Products.Sewing.ValueObjects;

namespace Catalog.Infrastructure.Adapters.Reference;

/// <summary>Tells Reference which fabrics, accessories and operations a product composition holds (ADR-0019).</summary>
internal sealed class CatalogPricingInputUsage(CatalogDbContext dbContext) : IPricingInputUsageSource
{
    public Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x =>
            x.SewingDetails != null && x.SewingDetails.Fabrics.Any(y => y.FabricId == new FabricId(fabricId)) ||
            x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Fabric && y.FabricId == new FabricId(fabricId)),
            cancellationToken);

    public Task<bool> IsAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x =>
            x.SewingDetails != null && x.SewingDetails.Accessories.Any(y => y.GarmentAccessoryId == new GarmentAccessoryId(accessoryId)) ||
            x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Accessory && y.AccessoryId == new GarmentAccessoryId(accessoryId)),
            cancellationToken);

    public Task<bool> IsOperationUsedAsync(int operationId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x =>
            x.SewingDetails != null && x.SewingDetails.Operations.Any(y => y.GarmentPartOperationId == new GarmentPartOperationId(operationId)) ||
            x.OperationLines.Any(y => y.OperationId == new GarmentPartOperationId(operationId)), cancellationToken);
}
