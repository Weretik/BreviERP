using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public const int SupplierSkuMaxLength = 200;
    public const int SupplierUrlMaxLength = 2048;
    public const int UnitMaxLength = 40;
    public const int PricingRuleCodeMaxLength = 200;

    /// <summary>
    /// Sets the PPE card fields from the sheet tab «ЗІЗ»: supplier, its product link and article, supplier price,
    /// «Дозволити ручну ціну», pricing rule and unit.
    /// </summary>
    public void SetPpeSupplier(PpeSupplierDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (Type != ProductType.Ppe)
            throw new DomainException(ProductErrors.OverviewInvalid("Supplier fields belong to a PPE product"));
        var url = Normalize(details.SupplierUrl);
        if (url is not null && (url.Length > SupplierUrlMaxLength || !IsWebUrl(url)))
            throw new DomainException(ProductErrors.OverviewInvalid("Supplier link must be an http(s) address"));
        if (details.SupplierPrice is <= 0)
            throw new DomainException(ProductErrors.OverviewInvalid("Supplier price must be positive"));
        if (Normalize(details.SupplierSku)?.Length > SupplierSkuMaxLength
            || Normalize(details.Unit)?.Length > UnitMaxLength
            || Normalize(details.PricingRuleCode)?.Length > PricingRuleCodeMaxLength)
            throw new DomainException(ProductErrors.OverviewInvalid("Supplier article, unit or rule is too long"));

        EnsureCatalogProfile().SetSupplier(details.SupplierId, details.SupplierSku, url, details.SupplierPrice,
            details.AllowManualPrice, details.PricingRuleCode, details.Unit);
        MarkAsUpdated(updatedAt);
    }
}
