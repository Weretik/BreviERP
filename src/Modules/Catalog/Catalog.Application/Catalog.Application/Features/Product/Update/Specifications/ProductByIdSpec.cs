using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Update.Specifications;

public sealed class ProductByIdSpec : Specification<ProductEntity>
{
    public ProductByIdSpec(int id) => Query.Where(x => x.Id.Value == id)
        .Include(x => x.Articles)
        .Include(x => x.MaterialLines)
        .Include(x => x.OperationLines)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Fabrics)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Accessories)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Operations);
}
