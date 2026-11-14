using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Api.Endpoints.SiteGalleries.Shared;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.SiteGalleries.SaveSiteWorks;

/// <summary>Replaces the list as a whole: 409 when the revision is stale.</summary>
internal static class SaveSiteWorksEndpoint
{
    private const string Key = SiteGalleryKeys.Works;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveSiteWorks");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteGalleryRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SaveSiteGalleryCommand(Key, request.Revision, request.Items, ContentEditor.By(user)),
            cancellationToken)).ToHttpResult();
}
