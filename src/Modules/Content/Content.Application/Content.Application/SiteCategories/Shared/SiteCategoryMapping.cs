using Content.Application.Contracts;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.SiteCategories.Shared;

internal static class SiteCategoryMapping
{
    public static SiteCategoryRow Row(SiteCategory category, SiteCategory? parent, int productCount)
    {
        var uk = category.TextIn(ProductPageRules.Ukrainian);
        var ru = category.TextIn(ProductPageRules.Russian);
        return new SiteCategoryRow(category.Id, category.ParentId, category.Slug, category.SortOrder,
            category.ShowOnSite, OnSite(category, parent), uk?.Name, ru?.Name, uk is { Text.Count: > 0 },
            ru is { Text.Count: > 0 }, uk is { HasSeo: true } && ru is { HasSeo: true }, productCount,
            category.UpdatedAtUtc);
    }

    public static bool OnSite(SiteCategory category, SiteCategory? parent) =>
        category.ShowOnSite && (category.ParentId is null || parent is { ShowOnSite: true });

    public static SiteCategoryTextResponse Text(SiteCategoryText? text) => text is null
        ? new SiteCategoryTextResponse(null, null, null, [])
        : new SiteCategoryTextResponse(text.Name, text.SeoTitle, text.SeoDescription, text.Text);

    public static SiteCategoryResponse Detail(SiteCategory category, SiteCategory? parent, int productCount) =>
        new(Row(category, parent, productCount), category.Revision, category.UpdatedBy,
            Text(category.TextIn(ProductPageRules.Ukrainian)), Text(category.TextIn(ProductPageRules.Russian)));
}
