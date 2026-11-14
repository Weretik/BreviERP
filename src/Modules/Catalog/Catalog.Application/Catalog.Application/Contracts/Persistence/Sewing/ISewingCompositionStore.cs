using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Sewing;

public interface ISewingCompositionStore
{
    Task<ProductEntity?> LoadAsync(int productId, CancellationToken cancellationToken);

    /// <summary>Whether another article or product already uses the sale SKU; <paramref name="exceptArticleId"/> is ignored.</summary>
    Task<bool> SaleSkuExistsAsync(string saleSku, long? exceptArticleId, CancellationToken cancellationToken);

    /// <summary>Whether the article already has saved calculations (price history).</summary>
    Task<bool> HasPriceHistoryAsync(long articleId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a fabric variant and its fabric line in one transaction: the article gets its ID first, then the line.
    /// False when the composition or the SKU changed meanwhile; a failure when a rule refuses the variant.
    /// </summary>
    Task<DomainResult<bool>> AddArticleAsync(ProductEntity product, SewingArticle article, FabricReferenceId fabricId,
        decimal fabricQuantity, DateTimeOffset now, CancellationToken cancellationToken);

    Task<bool> SaveAsync(CancellationToken cancellationToken);
}
