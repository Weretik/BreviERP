using Content.Application.Features.SiteGalleries.Shared.Public;
using Content.Application.Features.SiteGalleries.Queries.GetPublicSiteGalleries;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicSiteGalleries.GetPublicSiteGalleries;

internal static class GetPublicSiteGalleriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublicSiteGalleries")
            .WithSummary("Both lists; an empty list means the site keeps its own pictures")
            .WithDescription("Both lists; an empty list means the site keeps its own pictures. A file the site keeps has an address on the site itself (/assets/…); a library photo has its CDN address. A deleted library photo is left out.")
            .Produces<PublicSiteGalleriesResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicSiteGalleriesQuery(), cancellationToken)).ToHttpResult();
}
