using Catalog.Contracts.Products;
using Catalog.Domain.Products;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Catalog.Application.Features.Product.Queries.GetProductMediaUsages;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Contracts.Products;

/// <summary>Library photos and Reference rows that product cards use.</summary>
internal sealed class CatalogUsage(ISender sender, CatalogDbContext dbContext) : ICatalogUsage
{
    public async Task<IReadOnlyList<CatalogMediaUsage>> MediaUsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaUsagesQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog photo usages could not be read.");
        return result.Value.Select(x => new CatalogMediaUsage(x.MediaFileId, x.ProductId, x.ProductName)).ToArray();
    }

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
