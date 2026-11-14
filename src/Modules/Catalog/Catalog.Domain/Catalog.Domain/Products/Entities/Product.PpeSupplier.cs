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
    public DomainResult SetPpeSupplier(PpeSupplierDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (Type != ProductType.Ppe)
            return ProductErrors.OverviewInvalid("Поля постачальника — лише для товару ЗІЗ.");
        var url = Normalize(details.SupplierUrl);
        if (url is not null && (url.Length > SupplierUrlMaxLength || !IsWebUrl(url)))
            return ProductErrors.OverviewInvalid("Посилання постачальника — адреса http(s) до 2048 символів.");
        if (details.SupplierPrice is <= 0)
            return ProductErrors.OverviewInvalid("Ціна постачальника має бути більше нуля.");
        if (Normalize(details.SupplierSku)?.Length > SupplierSkuMaxLength
            || Normalize(details.Unit)?.Length > UnitMaxLength
            || Normalize(details.PricingRuleCode)?.Length > PricingRuleCodeMaxLength)
            return ProductErrors.OverviewInvalid("Артикул постачальника, одиниця або правило ціни задовгі.");

        EnsureCatalogProfile().SetSupplier(details.SupplierId, details.SupplierSku, url, details.SupplierPrice,
            details.AllowManualPrice, details.PricingRuleCode, details.Unit);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }
}
