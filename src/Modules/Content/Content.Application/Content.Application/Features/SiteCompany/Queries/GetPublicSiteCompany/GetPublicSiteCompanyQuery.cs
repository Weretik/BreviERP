using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Queries.GetPublicSiteCompany;

/// <summary>The company facts the public site shows (CNT-009 SC-03); not found before they exist.</summary>
public sealed record GetPublicSiteCompanyQuery : IQuery<Result<PublicSiteCompanyResponse>>;

public sealed class GetPublicSiteCompanyHandler(IPublicSiteCompanySource source)
    : IQueryHandler<GetPublicSiteCompanyQuery, Result<PublicSiteCompanyResponse>>
{
    public async ValueTask<Result<PublicSiteCompanyResponse>> Handle(GetPublicSiteCompanyQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken) is { } company ? company : Result.NotFound();
}
