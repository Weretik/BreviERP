using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public void SetModelCode(string? modelCode, DateTimeOffset updatedAt)
    {
        if (Type != ProductType.Sewing)
            throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        var normalized = string.IsNullOrWhiteSpace(modelCode) ? null : modelCode.Trim();
        if (normalized?.Length > 80)
            throw new DomainException(ProductErrors.ModelCodeInvalid());
        ModelCode = normalized;
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

    public void AddArticle(SewingArticle article, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        if (Type != ProductType.Sewing || article.ProductId != Id)
            throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        if (_articles.Count >= 200)
            throw new DomainException(ProductErrors.ArticleLimitReached());
        if (_articles.Any(x => string.Equals(x.SaleSku, article.SaleSku, StringComparison.Ordinal)))
            throw new DomainException(ProductErrors.DuplicateSaleSku(article.SaleSku));
        _articles.Add(article);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

}
