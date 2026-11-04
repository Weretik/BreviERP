using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Shared;

/// <summary>The PPE products of one supplier with their card fields, tracked so an import can save new prices.</summary>
public sealed class PpeSupplierProductsSpec : Specification<ProductEntity>
{
    public PpeSupplierProductsSpec(int supplierId)
    {
        var key = SupplierReferenceId.Create(supplierId);
        Query.Where(x => !x.IsDeleted && x.Type == ProductType.Ppe && x.CatalogProfile != null
                         && x.CatalogProfile.SupplierId == key)
            .Include(x => x.CatalogProfile);
    }
}

/// <summary>Supplier and «Дозволити ручну ціну» of every PPE product, for the counts in «Імпорт цін».</summary>
public sealed class PpeSupplierCountsSpec : Specification<ProductEntity, PpeSupplierCountRow>
{
    public PpeSupplierCountsSpec()
    {
        Query.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == ProductType.Ppe && x.CatalogProfile != null
                        && x.CatalogProfile.SupplierId != null)
            .Select(x => new PpeSupplierCountRow(x.CatalogProfile!.SupplierId!.Value, x.CatalogProfile.AllowManualPrice));
    }
}

public sealed record PpeSupplierCountRow(SupplierReferenceId SupplierId, bool AllowManualPrice);
