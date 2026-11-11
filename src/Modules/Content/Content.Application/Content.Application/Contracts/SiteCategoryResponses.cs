using Content.Domain.ProductPages;

namespace Content.Application.Contracts;

/// <summary>
/// A row of «Контент → Розділи». <paramref name="ParentId"/> is null for a category. <paramref name="OnSite"/> is true
/// only when the category and its parent are both marked. <paramref name="ProductCount"/> counts the products of the
/// category together with its subcategories. The <c>Has…</c> flags say what the site would miss.
/// </summary>
public sealed record SiteCategoryRow(
    int Id,
    int? ParentId,
    string Slug,
    int SortOrder,
    bool ShowOnSite,
    bool OnSite,
    string? NameUk,
    string? NameRu,
    bool HasUkrainianText,
    bool HasRussianText,
    bool HasSeo,
    int ProductCount,
    DateTimeOffset UpdatedAtUtc);

/// <summary>The whole tree: every category followed by its subcategories, in site order.</summary>
public sealed record SiteCategoryList(IReadOnlyList<SiteCategoryRow> Items);

/// <summary>Texts of one language; an empty list and nulls when nothing is written.</summary>
public sealed record SiteCategoryTextResponse(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock> Text);

/// <summary>The editor of one category: the row, the revision and texts by language (<c>uk</c>, <c>ru</c>).</summary>
public sealed record SiteCategoryResponse(
    SiteCategoryRow Category,
    int Revision,
    string? UpdatedBy,
    SiteCategoryTextResponse Uk,
    SiteCategoryTextResponse Ru);

public sealed record SiteCategoryVisibilityResponse(int Id, bool ShowOnSite, bool OnSite, int Revision);

/// <summary>
/// Result of the old-site category transfer (or what it would do when <paramref name="DryRun"/>): categories created,
/// categories whose texts were replaced, categories kept as they are, products added to categories, old-site
/// products without a Catalog product, and categories left out with the reason.
/// </summary>
public sealed record OldSiteCategoryImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    int ProductLinks,
    IReadOnlyList<OldSiteImportNote> NotMatched,
    IReadOnlyList<OldSiteImportNote> Problems);
