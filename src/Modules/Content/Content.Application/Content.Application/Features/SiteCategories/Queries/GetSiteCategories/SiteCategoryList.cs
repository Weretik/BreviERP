using Content.Domain.ProductPages;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategories;

/// <summary>The whole tree: every category followed by its subcategories, in site order.</summary>
public sealed record SiteCategoryList(IReadOnlyList<SiteCategoryRow> Items);
