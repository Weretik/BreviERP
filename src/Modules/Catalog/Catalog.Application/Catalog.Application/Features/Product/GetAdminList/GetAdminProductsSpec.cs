using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed class GetAdminProductsSpec : Specification<ProductEntity>
{
    public GetAdminProductsSpec() => Query.AsNoTracking().Include(x => x.Categories).Include(x => x.Photos);
}
