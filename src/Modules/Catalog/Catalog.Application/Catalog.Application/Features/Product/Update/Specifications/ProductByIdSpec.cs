using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Update.Specifications;

public sealed class ProductByIdSpec : Specification<ProductEntity>
{
    public ProductByIdSpec(int id) => Query.Where(x => x.Id.Value == id);
}
