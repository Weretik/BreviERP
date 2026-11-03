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

    /// <summary>
    /// Makes the article list equal to <paramref name="articles"/> (sheet order). An article with the same sale SKU
    /// is kept with its ID and takes the new activity and position; the others are added. Articles that are not in
    /// the list are removed together with their article-specific material lines.
    /// </summary>
    public void ReplaceArticles(IEnumerable<SewingArticle> articles, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(articles);
        if (Type != ProductType.Sewing)
            throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        var desired = articles.ToList();
        if (desired.Any(x => x.ProductId != Id))
            throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        if (desired.Count > 200)
            throw new DomainException(ProductErrors.ArticleLimitReached());
        var duplicate = desired.GroupBy(x => x.SaleSku, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
            throw new DomainException(ProductErrors.DuplicateSaleSku(duplicate.Key));

        var bySku = desired.ToDictionary(x => x.SaleSku, StringComparer.Ordinal);
        var removed = _articles.Where(x => !bySku.ContainsKey(x.SaleSku)).Select(x => x.Id).ToHashSet();
        _materialLines.RemoveAll(x => x.ArticleId.HasValue && removed.Contains(x.ArticleId.Value));
        _articles.RemoveAll(x => removed.Contains(x.Id));
        foreach (var article in desired)
        {
            var kept = _articles.FirstOrDefault(x => string.Equals(x.SaleSku, article.SaleSku, StringComparison.Ordinal));
            if (kept is null)
                _articles.Add(article);
            else
                kept.SetState(article.IsActive, article.SortOrder);
        }
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }
}
