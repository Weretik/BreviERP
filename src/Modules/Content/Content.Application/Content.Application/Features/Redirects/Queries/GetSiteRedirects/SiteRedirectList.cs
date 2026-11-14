using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetSiteRedirects;

public sealed record SiteRedirectList(IReadOnlyList<SiteRedirectResponse> Items);
