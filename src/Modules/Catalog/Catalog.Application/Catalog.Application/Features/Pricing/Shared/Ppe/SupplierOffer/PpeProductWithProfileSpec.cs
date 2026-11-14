using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

/// <summary>One PPE product with its catalog profile, which holds the supplier fields.</summary>
public sealed class PpeProductWithProfileSpec : Specification<ProductEntity>
{
    public PpeProductWithProfileSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId && x.Type == ProductType.Ppe).Include(x => x.CatalogProfile);
    }
}
