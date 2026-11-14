using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Shared.Photos;

public sealed class ProductSkuIndexSpec : Specification<ProductEntity, ProductSkuIndexRow>
{
    public ProductSkuIndexSpec()
    {
        Query.AsNoTracking().Where(x => !x.IsDeleted).Select(x => new ProductSkuIndexRow(
            x.Id.Value, x.Type, x.Name, x.CatalogProfile == null || x.CatalogProfile.IsActive,
            x.CatalogProfile != null ? x.CatalogProfile.SaleSku : null,
            x.Articles.Select(article => article.SaleSku).ToList()));
    }
}

public sealed record ProductSkuIndexRow(
    int Id, ProductType Type, string Name, bool IsActive, string? SaleSku, List<string> ArticleSkus);
