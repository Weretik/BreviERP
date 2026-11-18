using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.SiteGalleries.Shared.Public;

public interface IPublicSiteGalleriesSource
{
    Task<PublicSiteGalleriesResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>The pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011 R-06), read through <see cref="PublicSiteCache"/>.</summary>
public sealed class PublicSiteGalleriesSource(IContentDbContext db, IContentMediaReader media, IReadCache cache)
    : IPublicSiteGalleriesSource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-site-galleries");

    public async Task<PublicSiteGalleriesResponse> GetAsync(CancellationToken cancellationToken)
        => await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken);

    private async ValueTask<PublicSiteGalleriesResponse> ReadAsync(CancellationToken cancellationToken)
    {
        var galleries = await db.SiteGalleries.AsNoTracking().ToDictionaryAsync(x => x.Key, cancellationToken);
        var photos = await SiteGalleryReads.PhotosAsync(galleries.Values.SelectMany(x => x.Items), media, cancellationToken);
        return new PublicSiteGalleriesResponse(
            SiteGalleryReads.Public(galleries.GetValueOrDefault(SiteGallery.Works), photos),
            SiteGalleryReads.Public(galleries.GetValueOrDefault(SiteGallery.Certificates), photos));
    }
}
