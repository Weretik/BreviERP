
namespace Catalog.Domain.Products;

public sealed class SewingProductDetails
{
    private readonly List<ProductFabric> _fabrics = [];
    private readonly List<ProductAccessory> _accessories = [];
    private readonly List<ProductOperation> _operations = [];
    public ProductId ProductId { get; private set; }
    public decimal MetersPerProduct { get; private set; }
    public IReadOnlyCollection<ProductFabric> Fabrics => _fabrics.AsReadOnly();
    public IReadOnlyCollection<ProductAccessory> Accessories => _accessories.AsReadOnly();
    public IReadOnlyCollection<ProductOperation> Operations => _operations.AsReadOnly();

    private SewingProductDetails() { }

    private SewingProductDetails(decimal metersPerProduct, IReadOnlyCollection<ProductFabric> fabrics,
        IReadOnlyCollection<ProductAccessory> accessories, IReadOnlyCollection<ProductOperation> operations)
    {
        MetersPerProduct = metersPerProduct;
        _fabrics.AddRange(fabrics);
        _accessories.AddRange(accessories);
        _operations.AddRange(operations);
    }

    public static DomainResult<SewingProductDetails> Create(ProductId productId, decimal metersPerProduct, IEnumerable<ProductFabric> fabrics,
        IEnumerable<ProductAccessory> accessories, IEnumerable<ProductOperation> operations)
    {
        if (productId.Value == default) throw new ArgumentException("Sewing details need their product id.", nameof(productId));
        if (metersPerProduct <= 0) return ProductErrors.PositiveValueRequired(nameof(MetersPerProduct));
        ArgumentNullException.ThrowIfNull(fabrics);
        ArgumentNullException.ThrowIfNull(accessories);
        ArgumentNullException.ThrowIfNull(operations);
        var fabricList = fabrics.ToList();
        var accessoryList = accessories.ToList();
        var operationList = operations.ToList();
        if (fabricList.Any(x => x.ProductId != productId) ||
            accessoryList.Any(x => x.ProductId != productId) ||
            operationList.Any(x => x.ProductId != productId))
            return ProductErrors.DetailsBelongToAnotherProduct();
        var distinct = DomainResult.Combine(
            CheckDistinct(fabricList.Select(x => x.FabricId), "Fabric"),
            CheckDistinct(accessoryList.Select(x => x.GarmentAccessoryId), "Accessory"),
            CheckDistinct(operationList.Select(x => x.GarmentPartOperationId), "Operation"));
        if (distinct.IsFailure) return distinct.Error!;
        if (fabricList.Count(x => x.IsPrimary) > 2) return ProductErrors.TooManyPrimaryFabrics();
        var details = new SewingProductDetails(metersPerProduct, fabricList, accessoryList, operationList) { ProductId = productId };
        return details;
    }

    private static DomainResult CheckDistinct(IEnumerable<int> ids, string field)
    {
        var list = ids.ToList();
        if (list.Any(id => id <= 0)) return ProductErrors.PositiveValueRequired($"{field}Id");
        var duplicate = list.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
        return duplicate is null ? DomainResult.Success() : ProductErrors.DuplicateReference(field, duplicate.Key);
    }
}
