using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategories;

/// <summary>The tree of «Контент → Розділи»: every category followed by its subcategories (CNT-003 SC-01).</summary>
public sealed record GetSiteCategoriesQuery : IQuery<Result<SiteCategoryList>>;
