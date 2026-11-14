using Content.Domain.ProductPages;

namespace Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;

public sealed record SiteCategoryVisibilityResponse(int Id, bool ShowOnSite, bool OnSite, int Revision);
