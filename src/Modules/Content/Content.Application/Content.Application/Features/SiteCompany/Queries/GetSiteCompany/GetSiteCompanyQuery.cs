using Content.Application.Abstractions;
using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Queries.GetSiteCompany;

/// <summary>The company facts; the migration seeds them, so revision 0 only appears in an empty database (CNT-009 SC-01).</summary>
public sealed record GetSiteCompanyQuery : IQuery<Result<SiteCompanyResponse>>;
