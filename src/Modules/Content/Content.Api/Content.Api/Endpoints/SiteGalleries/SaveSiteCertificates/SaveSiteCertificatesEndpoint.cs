using Content.Application.Features.SiteGalleries.Shared.Admin;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Api.Endpoints.SiteGalleries.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteGalleries.SaveSiteCertificates;

/// <summary>Replaces the list as a whole: 409 when the revision is stale.</summary>
internal static class SaveSiteCertificatesEndpoint
{
    private const string Key = SiteGalleryKeys.Certificates;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveSiteCertificates")
            .WithSummary("Save site certificates")
            .WithDescription("Replaces the list as a whole. A new picture is a photo of the «Фото» library; a file the site still keeps may stay (by its legacyPath) but cannot be added.")
            .Produces<SiteGalleryResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteGalleryRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SaveSiteGalleryCommand(Key, request.Revision, request.Items, ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
