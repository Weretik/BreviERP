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
            x.SewingDetails != null && x.SewingDetails.Fabrics.Any(y => y.FabricId == fabricId) ||
            x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Fabric && y.FabricId == FabricReferenceId.Create(fabricId)),
            cancellationToken);

    public Task<bool> IsAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x =>
            x.SewingDetails != null && x.SewingDetails.Accessories.Any(y => y.GarmentAccessoryId == accessoryId) ||
            x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Accessory && y.AccessoryId == AccessoryReferenceId.Create(accessoryId)),
            cancellationToken);

    public Task<bool> IsOperationUsedAsync(int operationId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x =>
            x.SewingDetails != null && x.SewingDetails.Operations.Any(y => y.GarmentPartOperationId == operationId) ||
            x.OperationLines.Any(y => y.OperationId == OperationReferenceId.Create(operationId)), cancellationToken);
}
