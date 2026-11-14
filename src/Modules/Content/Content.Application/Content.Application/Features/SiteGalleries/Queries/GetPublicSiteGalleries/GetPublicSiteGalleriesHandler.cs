using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Public;

namespace Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;

public sealed class GetPublicSiteGalleriesHandler(IPublicSiteGalleriesSource source)
    : IQueryHandler<GetPublicSiteGalleriesQuery, Result<PublicSiteGalleriesResponse>>
{
    public async ValueTask<Result<PublicSiteGalleriesResponse>> Handle(GetPublicSiteGalleriesQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
