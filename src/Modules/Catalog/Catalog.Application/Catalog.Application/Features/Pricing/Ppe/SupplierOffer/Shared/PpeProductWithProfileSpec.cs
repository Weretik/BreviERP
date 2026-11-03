using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Shared;

/// <summary>One PPE product with its catalog profile, which holds the supplier fields.</summary>
public sealed class PpeProductWithProfileSpec : Specification<ProductEntity>
{
    public PpeProductWithProfileSpec(int id)
    {
        var productId = ProductId.Create(id);
        Query.Where(x => x.Id == productId && x.Type == ProductType.Ppe).Include(x => x.CatalogProfile);
    }
}
