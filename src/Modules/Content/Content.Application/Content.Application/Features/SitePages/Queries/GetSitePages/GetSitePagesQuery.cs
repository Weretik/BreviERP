using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Queries.GetSitePages;

/// <summary>Every fixed page of the site in menu order with its search texts (CNT-007 SC-01).</summary>
public sealed record GetSitePagesQuery : IQuery<Result<SitePageListResponse>>;
