using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

namespace Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

/// <summary>
/// A category of the old site: its address, the address of its parent (null on the top level), texts in both
/// languages and the old-site links (or last parts of them) of its products, in the order of the old site.
/// </summary>
public sealed record OldSiteCategory(
    string Slug,
    string? ParentSlug,
    SiteCategoryTextInput? Uk,
    SiteCategoryTextInput? Ru,
    IReadOnlyList<string>? ProductUrls);
