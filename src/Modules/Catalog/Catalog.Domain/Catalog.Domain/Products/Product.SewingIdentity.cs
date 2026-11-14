using Catalog.Domain.Products.Readiness;

namespace Catalog.Domain.Products;

public partial class Product
{
    public DomainResult SetModelCode(string? modelCode, DateTimeOffset updatedAt)
    {
        if (Type != ProductType.Sewing)
            return ProductErrors.TypeSpecificDataInvalid();
        var normalized = string.IsNullOrWhiteSpace(modelCode) ? null : modelCode.Trim();
        if (normalized?.Length > 80)
            return ProductErrors.ModelCodeInvalid();
        ModelCode = normalized;
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult AddArticle(SewingArticle article, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        if (Type != ProductType.Sewing || article.ProductId != Id)
            return ProductErrors.DetailsBelongToAnotherProduct();
        if (_articles.Count >= 200)
            return ProductErrors.ArticleLimitReached();
        if (_articles.Any(x => string.Equals(x.SaleSku, article.SaleSku, StringComparison.Ordinal)))
            return ProductErrors.DuplicateSaleSku(article.SaleSku);
        _articles.Add(article);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>Changes the sale SKU and the activity of one article (CAT-015).</summary>
    public DomainResult UpdateArticle(ArticleId articleId, string saleSku, bool isActive, DateTimeOffset updatedAt)
    {
        var article = _articles.FirstOrDefault(x => x.Id == articleId);
        if (article is null) return ProductErrors.ArticleNotFound();
        var sku = saleSku?.Trim() ?? string.Empty;
        if (_articles.Any(x => x.Id != articleId && string.Equals(x.SaleSku, sku, StringComparison.Ordinal)))
            return ProductErrors.DuplicateSaleSku(sku);
        var renamed = article.Rename(sku, isActive);
        if (renamed.IsFailure) return renamed;
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>Removes one article with its fabric line (CAT-015).</summary>
    public DomainResult RemoveArticle(ArticleId articleId, DateTimeOffset updatedAt)
    {
        if (_articles.RemoveAll(x => x.Id == articleId) == 0)
            return ProductErrors.ArticleNotFound();
        _materialLines.RemoveAll(x => x.ArticleId == articleId);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>
    /// Makes the article list equal to <paramref name="articles"/> (sheet order). An article with the same sale SKU
    /// is kept with its ID and takes the new activity and position; the others are added. Articles that are not in
    /// the list are removed together with their article-specific material lines.
    /// </summary>
    public DomainResult ReplaceArticles(IEnumerable<SewingArticle> articles, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(articles);
        if (Type != ProductType.Sewing)
            return ProductErrors.TypeSpecificDataInvalid();
        var desired = articles.ToList();
        if (desired.Any(x => x.ProductId != Id))
            return ProductErrors.DetailsBelongToAnotherProduct();
        if (desired.Count > 200)
            return ProductErrors.ArticleLimitReached();
        var duplicate = desired.GroupBy(x => x.SaleSku, StringComparer.Ordinal).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
            return ProductErrors.DuplicateSaleSku(duplicate.Key);

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
        return DomainResult.Success();
    }
}
