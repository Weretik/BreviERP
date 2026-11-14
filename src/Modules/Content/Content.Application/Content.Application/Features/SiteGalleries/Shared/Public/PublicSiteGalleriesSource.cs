using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.Features.SiteGalleries.Shared.Public;

public interface IPublicSiteGalleriesSource
{
    Task<PublicSiteGalleriesResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>The pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011 R-06), read at most once a minute.</summary>
public sealed class PublicSiteGalleriesSource(IContentDbContext db, IContentMediaReader media, IMemoryCache cache)
    : IPublicSiteGalleriesSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-site-galleries";

    public async Task<PublicSiteGalleriesResponse> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicSiteGalleriesResponse? cached) && cached is not null) return cached;
        var galleries = await db.SiteGalleries.AsNoTracking().ToDictionaryAsync(x => x.Key, cancellationToken);
        var photos = await SiteGalleryReads.PhotosAsync(galleries.Values.SelectMany(x => x.Items), media, cancellationToken);
        var result = new PublicSiteGalleriesResponse(
            SiteGalleryReads.Public(galleries.GetValueOrDefault(SiteGallery.Works), photos),
            SiteGalleryReads.Public(galleries.GetValueOrDefault(SiteGallery.Certificates), photos));
        cache.Set(Key, result, Lifetime);
        return result;
    }
}
