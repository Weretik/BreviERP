using Content.Api.SiteSettings;
using Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;

namespace Content.Api.Endpoints.SiteSettings;

/// <summary>Replaces the settings: 409 when the revision is stale or the picture is not in the library.</summary>
internal static class SaveSiteSeoSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveSiteSeoSettings");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteSeoSettingsRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SaveSiteSeoSettingsCommand(request.Revision, request.GoogleAnalyticsId,
            request.GoogleTagManagerId, request.GoogleSiteVerification, request.BingSiteVerification,
            request.DefaultOgPhotoId, ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
