using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>
/// Applies a draft through the Product domain methods. The planner runs it on a throwaway product to check the
/// domain rules; the store runs it on the stored product, so both take the same path.
/// </summary>
public static class SheetProductApplier
{
    /// <summary>
    /// Name (the Russian name column gets a copy), code, «Товари» fields, PPE supplier fields and the sewing article
    /// list. A sewing composition is cleared first; <see cref="ApplyComposition"/> fills it once articles have IDs.
    /// </summary>
    /// <param name="articleId">ID for a new article by its position; null lets the database pick it.</param>
    public static DomainResult ApplyCard(ProductEntity product, SheetProductDraft draft, DateTimeOffset now,
        Func<int, ArticleId>? articleId = null)
    {
        if (product.Name != draft.Name || product.RuName != draft.Name)
        {
            var renamed = product.Update(draft.Name, draft.Name, product.Slug, product.Type, now);
            if (renamed.IsFailure) return renamed;
        }

        var card = product.SetCode(draft.Code, now);
        if (card.IsSuccess) card = product.SetOverview(draft.Overview, now);
        if (card.IsSuccess && draft.Ppe is { } supplier) card = product.SetPpeSupplier(supplier, now);
        if (card.IsFailure || draft.Sewing is not { } sewing)
            return card;

        if (product.MaterialLines.Count > 0 || product.OperationLines.Count > 0)
        {
            var cleared = product.ReplaceComposition([], [], now);
            if (cleared.IsFailure) return cleared;
        }

        var articles = new List<SewingArticle>();
        foreach (var (article, index) in sewing.Articles.Select((article, index) => (article, index)))
        {
            var created = SewingArticle.Create(articleId?.Invoke(index) ?? default, product.Id, article.SaleSku,
                article.IsActive, index);
            if (created.IsFailure) return created.Error!;
            articles.Add(created.Value);
        }

        return product.ReplaceArticles(articles, now);
    }

    /// <summary>Replaces all material and operation lines; per-article lines find their article by sale SKU.</summary>
    public static DomainResult ApplyComposition(ProductEntity product, SewingCompositionDraft sewing, DateTimeOffset now)
    {
        var articles = product.Articles.ToDictionary(x => x.SaleSku, x => x.Id, StringComparer.Ordinal);
        var materials = new List<SewingMaterialLine>();
        foreach (var (line, index) in sewing.Materials.Select((line, index) => (line, index)))
        {
            var material = SewingMaterialLine.Create(default, product.Id,
                line.ArticleSku is null ? null : articles[line.ArticleSku], line.Kind,
                line.Kind == SewingMaterialKind.Fabric ? FabricReferenceId.Create(line.ReferenceId) : null,
                line.Kind == SewingMaterialKind.Accessory ? AccessoryReferenceId.Create(line.ReferenceId) : null,
                null, line.Quantity, line.Unit, index, line.Comment);
            if (material.IsFailure) return material.Error!;
            materials.Add(material.Value);
        }

        var operations = new List<SewingOperationLine>();
        foreach (var (line, index) in sewing.Operations.Select((line, index) => (line, index)))
        {
            var operation = SewingOperationLine.Create(default, product.Id, null,
                OperationReferenceId.Create(line.OperationId), line.Quantity, line.ManualMinutes, index, line.Comment);
            if (operation.IsFailure) return operation.Error!;
            operations.Add(operation.Value);
        }

        return product.ReplaceComposition(materials, operations, now);
    }
}
