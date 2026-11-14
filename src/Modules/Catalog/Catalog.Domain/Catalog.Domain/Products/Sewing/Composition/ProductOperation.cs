
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Sewing.Composition;

public sealed class ProductOperation
{
    public ProductId ProductId { get; private set; }
    public int GarmentPartOperationId { get; private set; }

    private ProductOperation() { }

    public static DomainResult<ProductOperation> Create(ProductId productId, int garmentPartOperationId)
    {
        if (productId.Value == default) throw new ArgumentException("An operation line needs its product id.", nameof(productId));
        if (garmentPartOperationId <= 0) return ProductErrors.PositiveValueRequired(nameof(GarmentPartOperationId));
        return new ProductOperation { ProductId = productId, GarmentPartOperationId = garmentPartOperationId };
    }
}
