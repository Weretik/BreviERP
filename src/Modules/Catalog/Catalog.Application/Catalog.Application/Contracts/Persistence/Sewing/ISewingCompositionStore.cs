using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Sewing;

public interface ISewingCompositionStore
{
    Task<ProductEntity?> LoadAsync(int productId, CancellationToken cancellationToken);
    Task<bool> SaleSkuExistsAsync(string saleSku, CancellationToken cancellationToken);
    Task<bool> SaveAsync(CancellationToken cancellationToken);
}
