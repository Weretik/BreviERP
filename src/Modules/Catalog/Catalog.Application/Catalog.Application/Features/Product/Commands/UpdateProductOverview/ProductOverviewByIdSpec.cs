using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Commands.UpdateProductOverview;

/// <summary>The product with its catalog profile, which holds the «Товари» page fields.</summary>
public sealed class ProductOverviewByIdSpec : Specification<ProductEntity>
{
    public ProductOverviewByIdSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId).Include(x => x.CatalogProfile);
    }
}
