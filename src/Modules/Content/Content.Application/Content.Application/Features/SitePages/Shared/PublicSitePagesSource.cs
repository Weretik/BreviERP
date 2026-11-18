using Content.Application.Abstractions;
using Content.Domain.SitePages;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.SitePages.Shared;

public interface IPublicSitePagesSource
{
    Task<PublicSitePagesResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>Texts of the fixed pages for the public site (CNT-007 R-03, CNT-010), read through <see cref="PublicSiteCache"/>.</summary>
public sealed class PublicSitePagesSource(IContentDbContext db, IReadCache cache) : IPublicSitePagesSource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-site-pages");

    public async Task<PublicSitePagesResponse> GetAsync(CancellationToken cancellationToken)
        => await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken);

    private async ValueTask<PublicSitePagesResponse> ReadAsync(CancellationToken cancellationToken)
    {
        var saved = await db.SitePages.AsNoTracking().ToListAsync(cancellationToken);
        var known = saved.Where(x => SitePageCatalog.Find(x.Key) is not null)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new PublicSitePage(x.Key, SitePageMapping.Uk(x), SitePageMapping.Ru(x)))
            .ToList();
        return new PublicSitePagesResponse(known);
    }
}
