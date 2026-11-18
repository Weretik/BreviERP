using BuildingBlocks.Domain.Identifiers.Products;
using BuildingBlocks.Domain.Identifiers.ReferenceData;

using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Sewing.Composition;

public sealed class ProductOperation
{
    public ProductId ProductId { get; private set; }
    public GarmentPartOperationId GarmentPartOperationId { get; private set; }

    private ProductOperation() { }

    public static DomainResult<ProductOperation> Create(ProductId productId, GarmentPartOperationId garmentPartOperationId)
    {
        if (productId.Value == default) throw new ArgumentException("An operation line needs its product id.", nameof(productId));
        if (garmentPartOperationId.Value <= 0) return ProductErrors.PositiveValueRequired(nameof(GarmentPartOperationId));
        return new ProductOperation { ProductId = productId, GarmentPartOperationId = garmentPartOperationId };
    }
}
