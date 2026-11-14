using Content.Application.Abstractions;
using Content.Domain.Articles;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.Features.Articles.Shared;

/// <param name="Articles">Articles marked «Показувати на сайті», the newest first.</param>
/// <param name="Photos">Library photos the articles use; a photo deleted from the library is missing.</param>
public sealed record PublicArticlesSnapshot(
    IReadOnlyList<Article> Articles,
    IReadOnlyDictionary<int, ContentMediaPhoto> Photos);

public interface IPublicArticlesSource
{
    Task<PublicArticlesSnapshot> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public article API answers from (CNT-004 R-07). It is kept for <see cref="Lifetime"/>, so an anonymous
/// caller cannot make the server read every article on each request, and a change in the Admin reaches the site
/// within that time.
/// </summary>
public sealed class PublicArticlesSource(IContentDbContext db, IContentMediaReader media, IMemoryCache cache)
    : IPublicArticlesSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-articles";

    public async Task<PublicArticlesSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicArticlesSnapshot? cached) && cached is not null) return cached;
        var articles = await db.Articles.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        var ids = articles.SelectMany(x => x.PhotoIds()).Distinct().ToArray();
        var photos = ids.Length == 0
            ? new Dictionary<int, ContentMediaPhoto>()
            : (await media.GetAsync(ids, cancellationToken)).ToDictionary(x => x.Id);
        var snapshot = new PublicArticlesSnapshot(articles, photos);
        cache.Set(Key, snapshot, Lifetime);
        return snapshot;
    }
}
