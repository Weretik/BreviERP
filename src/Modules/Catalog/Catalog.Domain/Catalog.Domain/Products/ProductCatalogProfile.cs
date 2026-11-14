
namespace Catalog.Domain.Products;

public sealed class ProductCatalogProfile
{
    public ProductId ProductId { get; private set; }
    public string? SaleSku { get; private set; }
    public string? Season { get; private set; }
    public bool IsActive { get; private set; }
    public string? Unit { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public SupplierReferenceId? SupplierId { get; private set; }
    public string? SupplierSku { get; private set; }
    public string? SupplierProductUrl { get; private set; }
    public decimal? SupplierPrice { get; private set; }
    public bool AllowManualPrice { get; private set; }
    public string? PricingRuleCode { get; private set; }
    public string? ShortDescription { get; private set; }
    /// <summary>Section ID from Reference «Типи і розділи»; no database relation across modules.</summary>
    public int? SectionId { get; private set; }

    private ProductCatalogProfile() { }

    public static ProductCatalogProfile Create(ProductId productId, bool isActive)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId.Value);
        return new ProductCatalogProfile { ProductId = productId, IsActive = isActive };
    }

    public void SetIdentity(string? saleSku, string? season, string? unit, string? websiteUrl)
    {
        SaleSku = Normalize(saleSku, 80);
        Season = Normalize(season, 100);
        Unit = Normalize(unit, 40);
        WebsiteUrl = Normalize(websiteUrl, 2048);
    }

    public void SetSupplierOffer(SupplierReferenceId supplierId, string? supplierSku, string? url,
        decimal? price, bool allowManualPrice, string? pricingRuleCode, string? shortDescription)
    {
        if (price is <= 0 || (price.HasValue && decimal.Round(price.Value, 6) != price))
            throw new ArgumentOutOfRangeException(nameof(price));
        SupplierId = supplierId;
        SupplierSku = Normalize(supplierSku, 200);
        SupplierProductUrl = Normalize(url, 2048);
        SupplierPrice = price;
        AllowManualPrice = allowManualPrice;
        PricingRuleCode = Normalize(pricingRuleCode, 200);
        ShortDescription = Normalize(shortDescription, 4000);
    }

    /// <summary>Sets the PPE supplier fields of the card; the quote description stays as it is.</summary>
    public void SetSupplier(SupplierReferenceId supplierId, string? supplierSku, string? url, decimal? price,
        bool allowManualPrice, string? pricingRuleCode, string? unit)
    {
        if (price is <= 0 || (price.HasValue && decimal.Round(price.Value, 6) != price))
            throw new ArgumentOutOfRangeException(nameof(price));
        SupplierId = supplierId;
        SupplierSku = Normalize(supplierSku, 200);
        SupplierProductUrl = Normalize(url, 2048);
        SupplierPrice = price;
        AllowManualPrice = allowManualPrice;
        PricingRuleCode = Normalize(pricingRuleCode, 200);
        Unit = Normalize(unit, 40);
    }

    /// <summary>Sets the «Товари» page fields; the season text is «Літо» or «Зима» as in the sheet.</summary>
    public void SetOverview(int? sectionId, string? season, bool isActive,
        string? websiteUrl, string? shortDescription)
    {
        if (sectionId is <= 0) throw new ArgumentOutOfRangeException(nameof(sectionId));
        SectionId = sectionId;
        Season = Normalize(season, 100);
        IsActive = isActive;
        WebsiteUrl = Normalize(websiteUrl, 2048);
        ShortDescription = Normalize(shortDescription, 4000);
    }

    public void SetSaleSku(string? saleSku) => SaleSku = Normalize(saleSku, 80);

    private static string? Normalize(string? value, int maxLength)
    {
        var text = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (text?.Length > maxLength)
            throw new ArgumentOutOfRangeException(nameof(value));
        return text;
    }
}
