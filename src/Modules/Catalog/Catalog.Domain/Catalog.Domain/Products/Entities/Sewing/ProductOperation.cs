using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class ProductOperation
{
    public ProductId ProductId { get; private set; }
    public int GarmentPartOperationId { get; private set; }

    private ProductOperation() { }

    public ProductOperation(ProductId productId, int garmentPartOperationId)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        if (garmentPartOperationId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(GarmentPartOperationId)));
        ProductId = productId;
        GarmentPartOperationId = garmentPartOperationId;
    }
}
