
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products;

public sealed class SewingArticle
{
    public ArticleId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string SaleSku { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    private SewingArticle() { }
    public static DomainResult<SewingArticle> Create(ArticleId id, ProductId productId, string saleSku, bool isActive = true, int sortOrder = 0)
    {
        if (id.Value < 0) throw new ArgumentException("An article id cannot be negative.", nameof(id));
        if (productId.Value <= 0) throw new ArgumentException("An article needs its product id.", nameof(productId));
        if (string.IsNullOrWhiteSpace(saleSku) || saleSku.Trim().Length > 80) return ProductErrors.SaleSkuInvalid();
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new SewingArticle { Id = id, ProductId = productId, SaleSku = saleSku.Trim(), IsActive = isActive, SortOrder = sortOrder };
    }

    /// <summary>Changes the sale SKU and the activity; the ID and the position stay.</summary>
    internal DomainResult Rename(string saleSku, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(saleSku) || saleSku.Trim().Length > 80) return ProductErrors.SaleSkuInvalid();
        SaleSku = saleSku.Trim();
        IsActive = isActive;
        return DomainResult.Success();
    }

    /// <summary>Changes the activity and the position of a kept article; the sale SKU and ID stay.</summary>
    internal void SetState(bool isActive, int sortOrder)
    {
        // The position comes from an article made by Create, which refuses a negative one.
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        IsActive = isActive;
        SortOrder = sortOrder;
    }
}
