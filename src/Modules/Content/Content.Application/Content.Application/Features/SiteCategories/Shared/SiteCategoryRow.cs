using Content.Domain.ProductPages;

namespace Content.Application.Features.SiteCategories.Shared;

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
