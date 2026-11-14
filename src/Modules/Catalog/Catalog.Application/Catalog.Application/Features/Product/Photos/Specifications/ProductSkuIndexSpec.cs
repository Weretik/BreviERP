using Catalog.Application.Features.Product.Photos.DTOs;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Photos.Specifications;

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
