using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public ProductCatalogProfile? CatalogProfile { get; private set; }

    public void SetCatalogProfile(ProductCatalogProfile profile, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.ProductId != Id || (Type == ProductType.Ppe && string.IsNullOrWhiteSpace(profile.SaleSku)))
            throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        CatalogProfile = profile;
        MarkAsUpdated(updatedAt);
    }

}
