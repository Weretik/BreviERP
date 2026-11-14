using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;

/// <summary>Pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011 SC-04).</summary>
public sealed record GetPublicSiteGalleriesQuery : IQuery<Result<PublicSiteGalleriesResponse>>;

public sealed class GetPublicSiteGalleriesHandler(IPublicSiteGalleriesSource source)
    : IQueryHandler<GetPublicSiteGalleriesQuery, Result<PublicSiteGalleriesResponse>>
{
    public async ValueTask<Result<PublicSiteGalleriesResponse>> Handle(GetPublicSiteGalleriesQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}
