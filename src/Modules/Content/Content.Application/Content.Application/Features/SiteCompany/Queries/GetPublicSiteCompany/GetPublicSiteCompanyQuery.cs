using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Queries.GetPublicSiteCompany;

/// <summary>The company facts the public site shows (CNT-009 SC-03); not found before they exist.</summary>
public sealed record GetPublicSiteCompanyQuery : IQuery<Result<PublicSiteCompanyResponse>>;
