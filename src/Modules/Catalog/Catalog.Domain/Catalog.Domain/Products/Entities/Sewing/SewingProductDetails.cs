using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

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

    public static SewingProductDetails Create(ProductId productId, decimal metersPerProduct, IEnumerable<ProductFabric> fabrics,
        IEnumerable<ProductAccessory> accessories, IEnumerable<ProductOperation> operations)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        if (metersPerProduct <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(MetersPerProduct)));
        ArgumentNullException.ThrowIfNull(fabrics);
        ArgumentNullException.ThrowIfNull(accessories);
        ArgumentNullException.ThrowIfNull(operations);
        var fabricList = fabrics.ToList();
        var accessoryList = accessories.ToList();
        var operationList = operations.ToList();
        if (fabricList.Any(x => x.ProductId != productId) ||
            accessoryList.Any(x => x.ProductId != productId) ||
            operationList.Any(x => x.ProductId != productId))
            throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        EnsureDistinct(fabricList.Select(x => x.FabricId), "Fabric");
        EnsureDistinct(accessoryList.Select(x => x.GarmentAccessoryId), "Accessory");
        EnsureDistinct(operationList.Select(x => x.GarmentPartOperationId), "Operation");
        if (fabricList.Count(x => x.IsPrimary) > 2) throw new DomainException(ProductErrors.TooManyPrimaryFabrics());
        var details = new SewingProductDetails(metersPerProduct, fabricList, accessoryList, operationList) { ProductId = productId };
        return details;
    }

    private static void EnsureDistinct(IEnumerable<int> ids, string field)
    {
        foreach (var id in ids)
            if (id <= 0) throw new DomainException(ProductErrors.PositiveValueRequired($"{field}Id"));
        var duplicate = ids.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) throw new DomainException(ProductErrors.DuplicateReference(field, duplicate.Key));
    }
}
