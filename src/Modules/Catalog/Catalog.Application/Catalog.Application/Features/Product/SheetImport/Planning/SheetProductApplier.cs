using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

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
    public static void ApplyCard(ProductEntity product, SheetProductDraft draft, DateTimeOffset now,
        Func<int, ArticleId>? articleId = null)
    {
        if (product.Name != draft.Name || product.RuName != draft.Name)
            product.Update(draft.Name, draft.Name, product.Slug, product.Type, now);
        product.SetCode(draft.Code, now);
        product.SetOverview(draft.Overview, now);
        if (draft.Ppe is { } supplier)
            product.SetPpeSupplier(supplier, now);
        if (draft.Sewing is not { } sewing)
            return;

        if (product.MaterialLines.Count > 0 || product.OperationLines.Count > 0)
            product.ReplaceComposition([], [], now);
        product.ReplaceArticles(sewing.Articles.Select((article, index) => SewingArticle.Create(
            articleId?.Invoke(index) ?? default, product.Id, article.SaleSku, article.IsActive, index)), now);
    }

    /// <summary>Replaces all material and operation lines; per-article lines find their article by sale SKU.</summary>
    public static void ApplyComposition(ProductEntity product, SewingCompositionDraft sewing, DateTimeOffset now)
    {
        var articles = product.Articles.ToDictionary(x => x.SaleSku, x => x.Id, StringComparer.Ordinal);
        var materials = sewing.Materials.Select((line, index) => SewingMaterialLine.Create(default, product.Id,
            line.ArticleSku is null ? null : articles[line.ArticleSku], line.Kind,
            line.Kind == SewingMaterialKind.Fabric ? FabricReferenceId.Create(line.ReferenceId) : null,
            line.Kind == SewingMaterialKind.Accessory ? AccessoryReferenceId.Create(line.ReferenceId) : null,
            null, line.Quantity, line.Unit, index, line.Comment)).ToList();
        var operations = sewing.Operations.Select((line, index) => SewingOperationLine.Create(default, product.Id,
            null, OperationReferenceId.Create(line.OperationId), line.Quantity, line.ManualMinutes, index,
            line.Comment)).ToList();
        product.ReplaceComposition(materials, operations, now);
    }
}
