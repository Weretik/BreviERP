using Catalog.Contracts.Products;
using Reference.Application.Contracts.Catalog;
using Reference.Application.Contracts.Pricing;

namespace Reference.Infrastructure.Adapters.Catalog;

/// <summary>
/// Before a delete: a Reference row is in use when a pricing version keeps it or a Catalog product composition holds it.
/// </summary>
internal sealed class ProductUsageReader(IReferencePricingUsageReader pricingUsage, ICatalogUsage catalog)
    : IProductUsageReader
{
    public async Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken) =>
        await pricingUsage.IsFabricVersionedAsync(fabricId, cancellationToken) ||
        await catalog.IsFabricUsedAsync(fabricId, cancellationToken);

    public async Task<bool> IsGarmentAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) =>
        await pricingUsage.IsAccessoryVersionedAsync(accessoryId, cancellationToken) ||
        await catalog.IsAccessoryUsedAsync(accessoryId, cancellationToken);

    public async Task<bool> IsGarmentPartOperationUsedAsync(int operationId, CancellationToken cancellationToken) =>
        await pricingUsage.IsOperationVersionedAsync(operationId, cancellationToken) ||
        await catalog.IsOperationUsedAsync(operationId, cancellationToken);
}
