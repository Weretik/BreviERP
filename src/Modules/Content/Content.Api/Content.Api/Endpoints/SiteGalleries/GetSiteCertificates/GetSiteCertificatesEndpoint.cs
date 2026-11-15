using Content.Application.Features.SiteGalleries.Shared.Admin;
using Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteGalleries.GetSiteCertificates;

internal static class GetSiteCertificatesEndpoint
{
    private const string Key = SiteGalleryKeys.Certificates;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSiteCertificates")
            .WithSummary("The documents in the order the site shows them; revision 0 only in an empty database")
            .Produces<SiteGalleryResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSiteGalleryQuery(Key), cancellationToken)).ToHttpResult();
}
