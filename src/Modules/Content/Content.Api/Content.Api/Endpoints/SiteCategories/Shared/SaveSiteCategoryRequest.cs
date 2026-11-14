using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

namespace Content.Api.Endpoints.SiteCategories.Shared;

/// <summary>
/// Body of <c>createSiteCategory</c> and <c>saveSiteCategory</c>; <c>Revision</c> is 0 for a new category,
/// <c>ParentId</c> is null for a category of the top level.
/// </summary>
public sealed record SaveSiteCategoryRequest(
    int Revision, int? ParentId, string? Slug, bool ShowOnSite, SiteCategoryTextInput? Uk, SiteCategoryTextInput? Ru);
