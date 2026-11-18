using Content.Application.Abstractions;
using Content.Domain.Articles;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.Articles.Shared.Public.Snapshot;

/// <param name="Articles">Articles marked «Показувати на сайті», the newest first.</param>
/// <param name="Photos">Library photos the articles use; a photo deleted from the library is missing.</param>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicArticlesSnapshot(
    IReadOnlyList<PublicArticleView> Articles,
    IReadOnlyDictionary<int, ContentMediaPhoto> Photos);

public interface IPublicArticlesSource
{
    Task<PublicArticlesSnapshot> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public article API answers from (CNT-004 R-07). It is read through <see cref="PublicSiteCache"/>, so an anonymous
/// caller cannot make the server read every article on each request, and a change in the Admin reaches the site
/// within that time.
/// </summary>
public sealed class PublicArticlesSource(IContentDbContext db, IContentMediaReader media, IReadCache cache)
    : IPublicArticlesSource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-articles");

    public async Task<PublicArticlesSnapshot> GetAsync(CancellationToken cancellationToken)
        => await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken);

    private async ValueTask<PublicArticlesSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        var articles = await db.Articles.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        var views = articles.Select(PublicArticleView.From).ToArray();
        var ids = views.SelectMany(x => x.PhotoIds).Distinct().ToArray();
        var photos = ids.Length == 0
            ? new Dictionary<int, ContentMediaPhoto>()
            : (await media.GetAsync(ids, cancellationToken)).ToDictionary(x => x.Id);
        return new PublicArticlesSnapshot(views, photos);
    }
}
