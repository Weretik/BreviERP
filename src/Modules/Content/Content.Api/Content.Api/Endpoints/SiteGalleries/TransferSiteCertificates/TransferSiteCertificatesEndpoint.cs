using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Api.Endpoints.SiteGalleries.TransferSiteCertificates;

/// <summary>Moves the next pictures the site keeps as its own files into the «Фото» library.</summary>
internal static class TransferSiteCertificatesEndpoint
{
    private const string Key = SiteGalleryKeys.Certificates;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("transfer", HandleAsync).WithName("transferSiteCertificates");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new TransferSiteGalleryCommand(Key, ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
