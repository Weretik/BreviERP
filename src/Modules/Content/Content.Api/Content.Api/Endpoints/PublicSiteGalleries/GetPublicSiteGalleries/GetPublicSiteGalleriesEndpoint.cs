using Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicSiteGalleries.GetPublicSiteGalleries;

internal static class GetPublicSiteGalleriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteGalleries");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteGalleriesQuery(), cancellationToken)).ToHttpResult();
}
