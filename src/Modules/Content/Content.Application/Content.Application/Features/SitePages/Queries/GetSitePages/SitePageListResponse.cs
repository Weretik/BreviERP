using Content.Domain.ProductPages;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Queries.GetSitePages;

public sealed record SitePageListResponse(IReadOnlyList<SitePageResponse> Items);
