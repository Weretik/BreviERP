using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

public sealed class ProductIdsByTypeSpec : Specification<ProductEntity, int>
{
    public ProductIdsByTypeSpec(ProductType type)
    {
        Query.AsNoTracking().Where(x => x.Type == type).OrderBy(x => x.Id);
        Query.Select(x => x.Id.Value);
    }
}
