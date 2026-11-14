using Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Api.Endpoints.SiteGalleries.GetSiteWorks;

internal static class GetSiteWorksEndpoint
{
    private const string Key = SiteGalleryKeys.Works;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteWorks");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteGalleryQuery(Key), cancellationToken)).ToHttpResult();
}
