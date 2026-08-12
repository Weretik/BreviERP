using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class PpeProductDetails
{
    public ProductId ProductId { get; private set; }
    public int SupplierId { get; private set; }
    public decimal BasePrice { get; private set; }
    public RetailPricePercent RetailPercent { get; private set; } = null!;
    public WholesalePricePercent WholesalePercent { get; private set; } = null!;

    private PpeProductDetails() { }

    private PpeProductDetails(int supplierId, decimal basePrice, RetailPricePercent retailPercent, WholesalePricePercent wholesalePercent)
    {
        SupplierId = supplierId;
        BasePrice = basePrice;
        RetailPercent = retailPercent;
        WholesalePercent = wholesalePercent;
    }

    public static PpeProductDetails Create(ProductId productId, int supplierId, decimal basePrice, RetailPricePercent retailPercent, WholesalePricePercent wholesalePercent)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        if (supplierId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SupplierId)));
        if (basePrice <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(BasePrice)));
        ArgumentNullException.ThrowIfNull(retailPercent);
        ArgumentNullException.ThrowIfNull(wholesalePercent);
        return new PpeProductDetails(supplierId, basePrice, retailPercent, wholesalePercent) { ProductId = productId };
    }
}
