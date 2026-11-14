using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;

namespace Content.Application.Features.Articles.Shared;

internal static class ArticleMapping
{
    public static ArticleRow Row(Article article, IReadOnlyDictionary<int, ContentMediaPhoto> photos)
    {
        var uk = article.TextIn(ProductPageRules.Ukrainian);
        var ru = article.TextIn(ProductPageRules.Russian);
        var cover = article.CoverPhotoId is { } id ? photos.GetValueOrDefault(id) : null;
        return new ArticleRow(article.Id, article.Slug, article.PublishedOn, article.ShowOnSite, uk?.Title, ru?.Title,
            uk is { Body.Count: > 0 }, ru is { Body.Count: > 0 }, uk is { HasSeo: true } && ru is { HasSeo: true },
            cover?.ThumbnailUrl ?? cover?.Url, article.UpdatedAtUtc);
    }

    public static ArticleTextResponse Text(ArticleText? text) => text is null
        ? new ArticleTextResponse(null, null, null, null, [])
        : new ArticleTextResponse(text.Title, text.Excerpt, text.SeoTitle, text.SeoDescription, text.Body);

    public static ArticleResponse Detail(Article article, IReadOnlyDictionary<int, ContentMediaPhoto> photos) =>
        new(Row(article, photos), article.Revision, article.UpdatedBy, article.CoverPhotoId,
            Text(article.TextIn(ProductPageRules.Ukrainian)), Text(article.TextIn(ProductPageRules.Russian)),
            article.PhotoIds().Where(photos.ContainsKey).Select(id => photos[id])
                .Select(x => new ArticlePhoto(x.Id, x.Url, x.ThumbnailUrl, x.Width, x.Height)).ToArray());
}
