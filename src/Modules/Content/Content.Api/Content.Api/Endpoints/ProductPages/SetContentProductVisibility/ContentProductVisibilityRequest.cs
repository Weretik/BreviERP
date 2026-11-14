using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

namespace Content.Api.Endpoints.ProductPages.SetContentProductVisibility;

public sealed record ContentProductVisibilityRequest(bool ShowOnSite);
