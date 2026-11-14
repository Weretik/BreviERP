using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed class ProductByIdOrNameSpec : Specification<ProductEntity>
{
    public ProductByIdOrNameSpec(int id, string name, string ruName, string slug)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId || x.Name == name || x.RuName == ruName || x.Slug == slug);
    }
}
