using Content.Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.Features.SiteSettings.Shared;

public interface IPublicSiteSettingsSource
{
    Task<PublicSiteSettingsResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>What the public site places on every page (CNT-006 R-04), read at most once a minute.</summary>
public sealed class PublicSiteSettingsSource(IContentDbContext db, IContentMediaReader media, IMemoryCache cache)
    : IPublicSiteSettingsSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-site-settings";

    public async Task<PublicSiteSettingsResponse> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicSiteSettingsResponse? cached) && cached is not null) return cached;
        var settings = await db.SiteSeoSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var picture = await SiteSettingsReads.PictureAsync(media, settings?.DefaultOgPhotoId, cancellationToken);
        var result = new PublicSiteSettingsResponse(settings?.GoogleAnalyticsId, settings?.GoogleTagManagerId,
            settings?.GoogleSiteVerification, settings?.BingSiteVerification,
            picture is null ? null : new PublicSitePicture(picture.Url, picture.Width, picture.Height));
        cache.Set(Key, result, Lifetime);
        return result;
    }
}
