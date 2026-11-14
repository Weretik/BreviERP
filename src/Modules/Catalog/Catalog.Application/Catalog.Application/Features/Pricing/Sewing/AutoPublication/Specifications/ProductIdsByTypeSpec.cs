using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Pricing.Sewing.AutoPublication.Specifications;

public sealed class ProductIdsByTypeSpec : Specification<ProductEntity, int>
{
    public ProductIdsByTypeSpec(ProductType type)
    {
        Query.AsNoTracking().Where(x => x.Type == type).OrderBy(x => x.Id);
        Query.Select(x => x.Id.Value);
    }
}
