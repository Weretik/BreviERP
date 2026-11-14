using Content.Application.Features.SitePages.Shared;

namespace Content.Api.Endpoints.SitePages.SaveSitePage;

public sealed record SaveSitePageRequest(int Revision, SitePageText Uk, SitePageText Ru);
