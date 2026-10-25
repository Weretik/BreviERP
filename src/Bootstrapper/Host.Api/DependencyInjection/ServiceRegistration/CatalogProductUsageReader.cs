using Catalog.Infrastructure.DataBase;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Reference.Application.Contracts.Catalog;

namespace Host.Api.DependencyInjection.ServiceRegistration;

internal sealed class CatalogProductUsageReader(CatalogDbContext dbContext,
    Reference.Application.Contracts.Pricing.IReferencePricingUsageReader pricingUsage) : IProductUsageReader
{
    public async Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken) =>
        await pricingUsage.IsFabricVersionedAsync(fabricId, cancellationToken) || await dbContext.Products.AnyAsync(x =>
        x.SewingDetails != null && x.SewingDetails.Fabrics.Any(y => y.FabricId == fabricId) ||
        x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Fabric && y.FabricId == FabricReferenceId.Create(fabricId)), cancellationToken);
    public async Task<bool> IsGarmentAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) =>
        await pricingUsage.IsAccessoryVersionedAsync(accessoryId, cancellationToken) || await dbContext.Products.AnyAsync(x =>
        x.SewingDetails != null && x.SewingDetails.Accessories.Any(y => y.GarmentAccessoryId == accessoryId) ||
        x.MaterialLines.Any(y => y.Kind == SewingMaterialKind.Accessory && y.AccessoryId == AccessoryReferenceId.Create(accessoryId)), cancellationToken);
    public async Task<bool> IsGarmentPartOperationUsedAsync(int operationId, CancellationToken cancellationToken) =>
        await pricingUsage.IsOperationVersionedAsync(operationId, cancellationToken) || await dbContext.Products.AnyAsync(x =>
        x.SewingDetails != null && x.SewingDetails.Operations.Any(y => y.GarmentPartOperationId == operationId) ||
        x.OperationLines.Any(y => y.OperationId == OperationReferenceId.Create(operationId)), cancellationToken);
}
