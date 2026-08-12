using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class ProductFabric
{
    public ProductId ProductId { get; private set; }
    public int FabricId { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    private ProductFabric() { }

    public ProductFabric(ProductId productId, int fabricId, bool isPrimary, int sortOrder)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        if (fabricId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(FabricId)));
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        ProductId = productId;
        FabricId = fabricId;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }
}
