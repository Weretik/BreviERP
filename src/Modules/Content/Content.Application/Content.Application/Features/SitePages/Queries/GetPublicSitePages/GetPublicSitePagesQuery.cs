using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Queries.GetPublicSitePages;

/// <summary>Search texts of the fixed pages of the public site (CNT-007 SC-03).</summary>
public sealed record GetPublicSitePagesQuery : IQuery<Result<PublicSitePagesResponse>>;
