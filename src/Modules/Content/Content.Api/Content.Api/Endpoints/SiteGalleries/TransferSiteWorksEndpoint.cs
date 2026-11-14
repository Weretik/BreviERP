using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Api.Endpoints.SiteGalleries;

/// <summary>Moves the next pictures the site keeps as its own files into the «Фото» library.</summary>
internal static class TransferSiteWorksEndpoint
{
    private const string Key = SiteGalleryKeys.Works;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("transfer", HandleAsync).WithName("transferSiteWorks");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new TransferSiteGalleryCommand(Key, ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
