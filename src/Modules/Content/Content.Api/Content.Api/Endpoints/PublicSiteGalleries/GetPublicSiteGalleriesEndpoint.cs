using Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;

namespace Content.Api.Endpoints.PublicSiteGalleries;

internal static class GetPublicSiteGalleriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteGalleries");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteGalleriesQuery(), cancellationToken)).ToHttpResult();
}
