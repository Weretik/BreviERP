using Content.Application.Abstractions;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.SiteSettings.Shared;

public interface IPublicSiteSettingsSource
{
    Task<PublicSiteSettingsResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>What the public site places on every page (CNT-006 R-04), read through <see cref="PublicSiteCache"/>.</summary>
public sealed class PublicSiteSettingsSource(IContentDbContext db, IContentMediaReader media, IReadCache cache)
    : IPublicSiteSettingsSource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-site-settings");

    public async Task<PublicSiteSettingsResponse> GetAsync(CancellationToken cancellationToken)
        => await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken);

    private async ValueTask<PublicSiteSettingsResponse> ReadAsync(CancellationToken cancellationToken)
    {
        var settings = await db.SiteSeoSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var picture = await SiteSettingsReads.PictureAsync(media, settings?.DefaultOgPhotoId?.Value, cancellationToken);
        return new PublicSiteSettingsResponse(settings?.GoogleAnalyticsId, settings?.GoogleTagManagerId,
            settings?.GoogleSiteVerification, settings?.BingSiteVerification,
            picture is null ? null : new PublicSitePicture(picture.Url, picture.Width, picture.Height));
    }
}
