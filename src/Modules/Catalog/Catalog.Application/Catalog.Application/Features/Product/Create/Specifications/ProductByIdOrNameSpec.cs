using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Create.Specifications;

public sealed class ProductByIdOrNameSpec : Specification<ProductEntity>
{
    public ProductByIdOrNameSpec(int id, string name, string ruName, string slug)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId || x.Name == name || x.RuName == ruName || x.Slug == slug);
    }
}
