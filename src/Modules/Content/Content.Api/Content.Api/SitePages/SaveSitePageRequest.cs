using Content.Application.Features.SitePages.Shared;

namespace Content.Api.SitePages;

public sealed record SaveSitePageRequest(int Revision, SitePageText Uk, SitePageText Ru);
