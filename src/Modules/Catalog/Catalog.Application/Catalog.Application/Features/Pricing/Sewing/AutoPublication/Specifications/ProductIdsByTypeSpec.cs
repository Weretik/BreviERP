using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Sewing.AutoPublication.Specifications;

public sealed class ProductIdsByTypeSpec : Specification<ProductEntity, int>
{
    public ProductIdsByTypeSpec(ProductType type)
    {
        Query.AsNoTracking().Where(x => x.Type == type).OrderBy(x => x.Id);
        Query.Select(x => x.Id.Value);
    }
}
