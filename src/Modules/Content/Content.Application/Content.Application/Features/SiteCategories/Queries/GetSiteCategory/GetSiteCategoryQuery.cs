using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategory;

/// <summary>The editor of one category or subcategory.</summary>
public sealed record GetSiteCategoryQuery(int Id) : IQuery<Result<SiteCategoryResponse>>;
