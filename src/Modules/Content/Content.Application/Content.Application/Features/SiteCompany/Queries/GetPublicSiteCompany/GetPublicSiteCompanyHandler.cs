using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Queries.GetPublicSiteCompany;

public sealed class GetPublicSiteCompanyHandler(IPublicSiteCompanySource source)
    : IQueryHandler<GetPublicSiteCompanyQuery, Result<PublicSiteCompanyResponse>>
{
    public async ValueTask<Result<PublicSiteCompanyResponse>> Handle(GetPublicSiteCompanyQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken) is { } company ? company : Result.NotFound();
}
