using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Shared.Photos;

/// <summary>The product with its photos; tracked so the photo list can be changed.</summary>
public sealed class ProductPhotosByIdSpec : Specification<ProductEntity>
{
    public ProductPhotosByIdSpec(int id, bool tracking)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId && !x.IsDeleted).Include(x => x.Photos);
        if (!tracking) Query.AsNoTracking();
    }
}
