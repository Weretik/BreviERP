using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Commands.DeleteProduct;

public sealed class ProductByIdSpec : Specification<ProductEntity>
{
    public ProductByIdSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId);
    }
}
