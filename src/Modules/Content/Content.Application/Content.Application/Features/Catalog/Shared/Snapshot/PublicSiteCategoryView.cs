using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.Features.Catalog.Shared.Snapshot;

/// <summary>
/// What the public catalog needs of a site category, copied out of the <see cref="SiteCategory"/> aggregate when the
/// snapshot is built (CQRS, PLT-005 R-02).
/// </summary>
public sealed record PublicSiteCategoryView(
    SiteCategoryId Id,
    SiteCategoryId? ParentId,
    string Slug,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<PublicSiteCategoryText> Texts)
{
    public PublicSiteCategoryText? TextIn(string language) => Texts.FirstOrDefault(x => x.Language == language);

    public static PublicSiteCategoryView From(SiteCategory category) =>
        new(category.Id, category.ParentId, category.Slug, category.UpdatedAtUtc,
            category.Texts.Select(x => new PublicSiteCategoryText(x.Language, x.Name, x.SeoTitle, x.SeoDescription, x.Text))
                .ToArray());
}

/// <summary>The texts of a site category in one language, as values.</summary>
public sealed record PublicSiteCategoryText(
    string Language,
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock> Text);
