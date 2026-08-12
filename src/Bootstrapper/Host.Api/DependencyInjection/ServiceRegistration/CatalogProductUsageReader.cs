using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Reference.Application.Contracts.Catalog;

namespace Host.Api.DependencyInjection.ServiceRegistration;

internal sealed class CatalogProductUsageReader(CatalogDbContext dbContext) : IProductUsageReader
{
    public Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken) => dbContext.Products.AnyAsync(x => x.SewingDetails != null && x.SewingDetails.Fabrics.Any(y => y.FabricId == fabricId), cancellationToken);
    public Task<bool> IsGarmentAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) => dbContext.Products.AnyAsync(x => x.SewingDetails != null && x.SewingDetails.Accessories.Any(y => y.GarmentAccessoryId == accessoryId), cancellationToken);
    public Task<bool> IsGarmentPartOperationUsedAsync(int operationId, CancellationToken cancellationToken) => dbContext.Products.AnyAsync(x => x.SewingDetails != null && x.SewingDetails.Operations.Any(y => y.GarmentPartOperationId == operationId), cancellationToken);
}
