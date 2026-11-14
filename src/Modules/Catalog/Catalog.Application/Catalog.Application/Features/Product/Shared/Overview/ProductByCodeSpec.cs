using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Shared.Overview;

/// <summary>A product of the type with this code: model ID for sewing, site article for PPE.</summary>
public sealed class ProductByCodeSpec : Specification<ProductEntity>
{
    public ProductByCodeSpec(ProductType type, string code)
    {
        if (type == ProductType.Ppe)
            Query.Where(x => x.CatalogProfile != null && x.CatalogProfile.SaleSku == code);
        else
            Query.Where(x => x.ModelCode == code);
        Query.AsNoTracking();
    }
}
