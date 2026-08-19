using ProductEntity = Catalog.Domain.Products.Entities.Product;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Delete.Specifications;

public sealed class ProductByIdSpec : Specification<ProductEntity>
{
    public ProductByIdSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId);
    }
}
