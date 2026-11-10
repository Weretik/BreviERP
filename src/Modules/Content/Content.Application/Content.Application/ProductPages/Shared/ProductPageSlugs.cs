using Content.Application.Abstractions;
using Content.Domain.ProductPages;

namespace Content.Application.ProductPages.Shared;

/// <summary>Picks a free address for a page that has none yet.</summary>
internal static class ProductPageSlugs
{
    /// <summary>
    /// The slug of the name; when it is taken or too short, the same with the product id at the end, which is always
    /// free because ids are unique.
    /// </summary>
    public static async Task<string> FreeAsync(IContentDbContext db, ContentCatalogProduct product,
        CancellationToken cancellationToken)
    {
        var slug = ProductPageRules.SlugFrom(product.Name);
        if (ProductPageRules.IsValidSlug(slug) && !await TakenAsync(db, slug, product.Id, cancellationToken)) return slug;
        var withId = ProductPageRules.SlugFrom($"{product.Name} {product.Id}");
        return ProductPageRules.IsValidSlug(withId) ? withId : $"product-{product.Id}";
    }

    public static Task<bool> TakenAsync(IContentDbContext db, string slug, int productId,
        CancellationToken cancellationToken) =>
        db.ProductPages.AnyAsync(x => x.Slug == slug && x.ProductId != productId, cancellationToken);
}
