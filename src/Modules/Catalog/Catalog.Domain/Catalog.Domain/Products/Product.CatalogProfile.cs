using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products;

public partial class Product
{
    public ProductCatalogProfile? CatalogProfile { get; private set; }

    public DomainResult SetCatalogProfile(ProductCatalogProfile profile, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.ProductId != Id || (Type == ProductType.Ppe && string.IsNullOrWhiteSpace(profile.SaleSku)))
            return ProductErrors.DetailsBelongToAnotherProduct();
        CatalogProfile = profile;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

}
