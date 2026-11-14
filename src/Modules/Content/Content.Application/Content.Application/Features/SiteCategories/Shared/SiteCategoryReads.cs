using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.Features.SiteCategories.Shared;

/// <summary>Reads shared by the handlers of «Контент → Розділи».</summary>
internal static class SiteCategoryReads
{
    /// <summary>The editor answer of one category with its parent and the number of its products.</summary>
    public static async Task<SiteCategoryResponse> DetailAsync(IContentDbContext db, SiteCategory category,
        CancellationToken cancellationToken)
    {
        var parent = category.ParentId is { } parentId
            ? await db.SiteCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken)
            : null;
        var ids = await db.SiteCategories.Where(x => x.ParentId == category.Id).Select(x => x.Id)
            .ToListAsync(cancellationToken);
        ids.Add(category.Id);
        var count = await db.SiteCategoryProducts.Where(x => ids.Contains(x.CategoryId)).Select(x => x.ProductId)
            .Distinct().CountAsync(cancellationToken);
        return SiteCategoryMapping.Detail(category, parent, count);
    }

    public static Task<bool> SlugTakenAsync(IContentDbContext db, string slug, int? categoryId,
        CancellationToken cancellationToken) =>
        db.SiteCategories.AnyAsync(x => x.Slug == slug && x.Id != (categoryId ?? 0), cancellationToken);

    /// <summary>The slug of the name; when it is taken or too short, the same with a number at the end.</summary>
    public static async Task<string> FreeSlugAsync(IContentDbContext db, string? name, CancellationToken cancellationToken)
    {
        var slug = ProductPageRules.SlugFrom(name);
        if (!ProductPageRules.IsValidSlug(slug)) slug = ProductPageRules.SlugFrom($"rozdil {name}");
        if (!await SlugTakenAsync(db, slug, null, cancellationToken)) return slug;
        for (var number = 2; ; number++)
        {
            var suffix = $"-{number}";
            var next = slug.Length + suffix.Length > ProductPageRules.SlugMaxLength
                ? slug[..(ProductPageRules.SlugMaxLength - suffix.Length)].TrimEnd('-') + suffix
                : slug + suffix;
            if (!await SlugTakenAsync(db, next, null, cancellationToken)) return next;
        }
    }

    public static async Task<int> NextSortOrderAsync(IContentDbContext db, int? parentId, CancellationToken cancellationToken) =>
        (await db.SiteCategories.Where(x => x.ParentId == parentId).Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken) ?? 0) + 1;
}
