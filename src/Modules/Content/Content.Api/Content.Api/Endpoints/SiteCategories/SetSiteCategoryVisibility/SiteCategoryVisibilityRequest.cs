using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

namespace Content.Api.Endpoints.SiteCategories.SetSiteCategoryVisibility;

public sealed record SiteCategoryVisibilityRequest(bool ShowOnSite);
