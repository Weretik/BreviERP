
namespace Catalog.Domain.Products;

public sealed class ProductAccessory
{
    public ProductId ProductId { get; private set; }
    public int GarmentAccessoryId { get; private set; }
    public decimal Quantity { get; private set; }
    public int SortOrder { get; private set; }

    private ProductAccessory() { }

    public static DomainResult<ProductAccessory> Create(ProductId productId, int garmentAccessoryId, decimal quantity, int sortOrder)
    {
        if (productId.Value == default) throw new ArgumentException("An accessory line needs its product id.", nameof(productId));
        if (garmentAccessoryId <= 0) return ProductErrors.PositiveValueRequired(nameof(GarmentAccessoryId));
        if (quantity <= 0) return ProductErrors.PositiveValueRequired(nameof(Quantity));
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new ProductAccessory { ProductId = productId, GarmentAccessoryId = garmentAccessoryId, Quantity = quantity, SortOrder = sortOrder };
    }
}
