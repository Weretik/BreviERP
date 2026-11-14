using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Queries.GetPublicSitePages;

public sealed class GetPublicSitePagesHandler(IPublicSitePagesSource source)
    : IQueryHandler<GetPublicSitePagesQuery, Result<PublicSitePagesResponse>>
{
    public async ValueTask<Result<PublicSitePagesResponse>> Handle(GetPublicSitePagesQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
