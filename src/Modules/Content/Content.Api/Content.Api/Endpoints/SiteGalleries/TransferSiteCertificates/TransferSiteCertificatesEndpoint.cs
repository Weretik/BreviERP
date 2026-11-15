using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteGalleries.TransferSiteCertificates;

/// <summary>Moves the next pictures the site keeps as its own files into the «Фото» library.</summary>
internal static class TransferSiteCertificatesEndpoint
{
    private const string Key = SiteGalleryKeys.Certificates;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("transfer", HandleAsync).WithName("transferSiteCertificates")
            .WithSummary("Transfer site certificates")
            .WithDescription("Moves up to 10 pictures the site still keeps as its own files into the «Фото» library (folder «Сайт»), keeping the order and the texts. The Admin repeats the call while moved > 0 and left > 0. A picture already in the library is not downloaded again.")
            .Produces<SiteGalleryTransferResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new TransferSiteGalleryCommand(Key, ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
