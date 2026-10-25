using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class ProductCatalogProfile
{
    public ProductId ProductId { get; private set; }
    public string? SaleSku { get; private set; }
    public string? Season { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSourceReviewed { get; private set; }
    public string? SourceReviewDate { get; private set; }
    public string? Unit { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public SupplierReferenceId? SupplierId { get; private set; }
    public string? SupplierSku { get; private set; }
    public string? SupplierProductUrl { get; private set; }
    public decimal? SupplierPrice { get; private set; }
    public bool AllowManualPrice { get; private set; }
    public string? PricingRuleCode { get; private set; }
    public string? ShortDescription { get; private set; }

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

    public void SetSourceReview(bool reviewed, string? originalDate)
    {
        IsSourceReviewed = reviewed;
        SourceReviewDate = Normalize(originalDate, 100);
    }

    private static string? Normalize(string? value, int maxLength)
    {
        var text = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (text?.Length > maxLength)
            throw new ArgumentOutOfRangeException(nameof(value));
        return text;
    }
}
