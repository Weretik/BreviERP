using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class ProductAccessory
{
    public ProductId ProductId { get; private set; }
    public int GarmentAccessoryId { get; private set; }
    public decimal Quantity { get; private set; }
    public int SortOrder { get; private set; }

    private ProductAccessory() { }

    public ProductAccessory(ProductId productId, int garmentAccessoryId, decimal quantity, int sortOrder)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        if (garmentAccessoryId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(GarmentAccessoryId)));
        if (quantity <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(Quantity)));
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        ProductId = productId;
        GarmentAccessoryId = garmentAccessoryId;
        Quantity = quantity;
        SortOrder = sortOrder;
    }
}
