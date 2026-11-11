using Content.Domain.SiteCategories;

namespace Content.Api.SiteCategories;

/// <summary>
/// Body of <c>createSiteCategory</c> and <c>saveSiteCategory</c>; <c>Revision</c> is 0 for a new category,
/// <c>ParentId</c> is null for a category of the top level.
/// </summary>
public sealed record SaveSiteCategoryRequest(
    int Revision, int? ParentId, string? Slug, bool ShowOnSite, SiteCategoryTextInput? Uk, SiteCategoryTextInput? Ru);

public sealed record SiteCategoryVisibilityRequest(bool ShowOnSite);

/// <summary>Body of <c>reorderSiteCategories</c>: all categories of one parent in the new order.</summary>
public sealed record ReorderSiteCategoriesRequest(int? ParentId, IReadOnlyList<int> Ids);
