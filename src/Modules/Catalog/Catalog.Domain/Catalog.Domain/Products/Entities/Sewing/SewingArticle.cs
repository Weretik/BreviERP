using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class SewingArticle
{
    public ArticleId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string SaleSku { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    private SewingArticle() { }
    public static SewingArticle Create(ArticleId id, ProductId productId, string saleSku, bool isActive = true, int sortOrder = 0)
    {
        if (id.Value < 0 || productId.Value <= 0) throw new DomainException(ProductErrors.IdIsRequired());
        if (string.IsNullOrWhiteSpace(saleSku) || saleSku.Trim().Length > 80) throw new DomainException(ProductErrors.SaleSkuInvalid());
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        return new SewingArticle { Id = id, ProductId = productId, SaleSku = saleSku.Trim(), IsActive = isActive, SortOrder = sortOrder };
    }
}
