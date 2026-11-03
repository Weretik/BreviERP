using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Update.Specifications;

/// <summary>The product with its catalog profile, which holds the «Товари» page fields.</summary>
public sealed class ProductOverviewByIdSpec : Specification<ProductEntity>
{
    public ProductOverviewByIdSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId).Include(x => x.CatalogProfile);
    }
}
