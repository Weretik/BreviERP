using BuildingBlocks.Domain.Identifiers.Products;
using Content.Application.Abstractions;
using Content.Domain.SiteCategories;

namespace Content.Application.Features.ProductPages.Shared;

/// <summary>The site categories of one product (CNT-003 R-03).</summary>
internal static class ProductCategoryLinks
{
    public static async Task<IReadOnlyList<int>> IdsAsync(IContentDbContext db, int productId,
        CancellationToken cancellationToken) =>
        await db.SiteCategoryProducts.Where(x => x.ProductId == new ProductId(productId)).OrderBy(x => x.CategoryId)
            .Select(x => x.CategoryId.Value).ToListAsync(cancellationToken);

    /// <summary>
    /// Makes the product belong to exactly these categories; a new link goes to the end of its category. Nothing is
    /// saved here. False when a category does not exist.
    /// </summary>
    public static async Task<bool> ReplaceAsync(IContentDbContext db, int productId, IReadOnlyList<int> categoryIds,
        CancellationToken cancellationToken)
    {
        var ids = categoryIds.Select(x => new SiteCategoryId(x)).ToList();
        var existing = await db.SiteCategories.Where(x => ids.Contains(x.Id)).Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (existing.Count != categoryIds.Count) return false;

        var links = await db.SiteCategoryProducts.Where(x => x.ProductId == new ProductId(productId)).ToListAsync(cancellationToken);
        db.SiteCategoryProducts.RemoveRange(links.Where(x => !ids.Contains(x.CategoryId)));
        foreach (var categoryId in ids.Where(id => links.All(x => x.CategoryId != id)))
        {
            var last = await db.SiteCategoryProducts.Where(x => x.CategoryId == categoryId)
                .Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0;
            db.SiteCategoryProducts.Add(SiteCategoryProduct.Create(categoryId, new ProductId(productId), last + 1));
        }

        return true;
    }
}
