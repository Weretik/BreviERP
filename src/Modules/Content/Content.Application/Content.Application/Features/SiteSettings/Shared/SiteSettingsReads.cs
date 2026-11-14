using Content.Application.Abstractions;
using Content.Domain.SiteSettings;

namespace Content.Application.Features.SiteSettings.Shared;

internal static class SiteSettingsReads
{
    public static async Task<SitePicture?> PictureAsync(IContentMediaReader media, int? photoId,
        CancellationToken cancellationToken)
    {
        if (photoId is not { } id) return null;
        var photo = (await media.GetAsync([id], cancellationToken)).FirstOrDefault();
        return photo is null ? null : new SitePicture(photo.Id, photo.Url, photo.Width, photo.Height);
    }

    public static SiteSeoSettingsResponse Response(SiteSeoSettings? settings, SitePicture? picture) =>
        settings is null
            ? new SiteSeoSettingsResponse(null, null, null, null, null, 0, null, null)
            : new SiteSeoSettingsResponse(settings.GoogleAnalyticsId, settings.GoogleTagManagerId,
                settings.GoogleSiteVerification, settings.BingSiteVerification, picture, settings.Revision,
                settings.UpdatedAtUtc, settings.UpdatedBy);
}
