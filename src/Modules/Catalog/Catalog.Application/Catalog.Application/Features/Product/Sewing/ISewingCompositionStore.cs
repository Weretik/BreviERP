using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Sewing;

public interface ISewingCompositionStore
{
    Task<ProductEntity?> LoadAsync(int productId, CancellationToken cancellationToken);
    Task<SewingArticleList?> ReadArticlesAsync(int productId, CancellationToken cancellationToken);
    Task<SewingComposition?> ReadCompositionAsync(int productId, long? articleId, CancellationToken cancellationToken);
    Task<bool> SaleSkuExistsAsync(string saleSku, CancellationToken cancellationToken);
    Task<bool> SaveAsync(CancellationToken cancellationToken);
}
