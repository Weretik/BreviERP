using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.Features.SitePages.Shared;

public interface IPublicSitePagesSource
{
    Task<PublicSitePagesResponse> GetAsync(CancellationToken cancellationToken);
}

/// <summary>Texts of the fixed pages for the public site (CNT-007 R-03, CNT-010), read at most once a minute.</summary>
public sealed class PublicSitePagesSource(IContentDbContext db, IMemoryCache cache) : IPublicSitePagesSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-site-pages";

    public async Task<PublicSitePagesResponse> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicSitePagesResponse? cached) && cached is not null) return cached;
        var saved = await db.SitePages.AsNoTracking().ToListAsync(cancellationToken);
        var known = saved.Where(x => SitePageCatalog.Find(x.Key) is not null)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new PublicSitePage(x.Key, SitePageMapping.Uk(x), SitePageMapping.Ru(x)))
            .ToList();
        var result = new PublicSitePagesResponse(known);
        cache.Set(Key, result, Lifetime);
        return result;
    }
}
