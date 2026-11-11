using Content.Application.ProductPages.ImportOldSite;
using Content.Domain.ProductPages;

namespace Content.Api.ProductPages;

/// <summary>
/// Body of <c>saveContentProduct</c>; <c>Revision</c> is 0 for a product without a page. <c>CategoryIds</c> replaces the
/// site categories of the product; null keeps them.
/// </summary>
public sealed record SaveContentProductRequest(
    int Revision, string? Slug, bool ShowOnSite, ProductPageTextInput? Uk, ProductPageTextInput? Ru,
    IReadOnlyList<int>? CategoryIds = null);

public sealed record ContentProductVisibilityRequest(bool ShowOnSite);

/// <summary>Body of <c>importOldSiteProductPages</c>; without <c>DryRun = false</c> nothing is written.</summary>
public sealed record ImportOldSiteProductPagesRequest(
    IReadOnlyList<OldSiteProductPage> Items, bool DryRun = true, bool ShowOnSite = false, bool Overwrite = false);
