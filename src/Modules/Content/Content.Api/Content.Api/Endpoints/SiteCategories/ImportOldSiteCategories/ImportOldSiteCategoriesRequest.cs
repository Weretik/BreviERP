using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

namespace Content.Api.Endpoints.SiteCategories.ImportOldSiteCategories;

/// <summary>Body of <c>importOldSiteCategories</c>; without <c>DryRun = false</c> nothing is written.</summary>
public sealed record ImportOldSiteCategoriesRequest(
    IReadOnlyList<OldSiteCategory> Items, bool DryRun = true, bool ShowOnSite = false, bool Overwrite = false);
