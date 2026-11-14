using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Commands.ReplaceProduct;

public sealed class ProductByIdOrNameExceptIdSpec : Specification<ProductEntity>
{
    public ProductByIdOrNameExceptIdSpec(int id, string name, string ruName, string slug)
        => Query.Where(x => x.Id.Value != id && (x.Name == name || x.RuName == ruName || x.Slug == slug));
}
