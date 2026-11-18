using BuildingBlocks.Domain.Identifiers.Products;
using BuildingBlocks.Domain.Identifiers.ReferenceData;

using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Sewing.Composition;

public sealed class ProductFabric
{
    public ProductId ProductId { get; private set; }
    public FabricId FabricId { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    private ProductFabric() { }

    public static DomainResult<ProductFabric> Create(ProductId productId, FabricId fabricId, bool isPrimary, int sortOrder)
    {
        if (productId.Value == default) throw new ArgumentException("A fabric line needs its product id.", nameof(productId));
        if (fabricId.Value <= 0) return ProductErrors.PositiveValueRequired(nameof(FabricId));
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new ProductFabric { ProductId = productId, FabricId = fabricId, IsPrimary = isPrimary, SortOrder = sortOrder };
    }
}
