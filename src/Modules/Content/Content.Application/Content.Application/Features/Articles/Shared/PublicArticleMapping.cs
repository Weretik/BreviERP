using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;

namespace Content.Application.Features.Articles.Shared;

internal static class PublicArticleMapping
{
    /// <summary>
    /// The text for a language: its own when it has a title and a body, otherwise the Ukrainian one, which every
    /// article has (its title is required).
    /// </summary>
    public static ArticleText TextFor(Article article, string language)
    {
        var own = article.TextIn(language);
        return own is { Title: not null, Body.Count: > 0 } ? own : article.TextIn(ProductPageRules.Ukrainian)!;
    }

    public static bool HasRussian(Article article) =>
        TextFor(article, ProductPageRules.Russian).Language == ProductPageRules.Russian;

    public static PublicArticleCard Card(Article article, string language,
        IReadOnlyDictionary<int, ContentMediaPhoto> photos)
    {
        var text = TextFor(article, language);
        return new PublicArticleCard(article.Slug, article.PublishedOn, text.Language, text.Title!, text.Excerpt,
            Cover(article, photos), HasRussian(article), article.UpdatedAtUtc);
    }

    public static PublicArticle Page(Article article, string language,
        IReadOnlyDictionary<int, ContentMediaPhoto> photos)
    {
        var text = TextFor(article, language);
        return new PublicArticle(article.Slug, article.PublishedOn, text.Language, HasRussian(article), text.Title!,
            text.Excerpt, text.SeoTitle, text.SeoDescription, Cover(article, photos),
            text.Body.Select(x => Block(x, photos)).Where(x => x is not null).Select(x => x!).ToArray(),
            article.UpdatedAtUtc);
    }

    private static PublicArticlePhoto? Cover(Article article, IReadOnlyDictionary<int, ContentMediaPhoto> photos) =>
        article.CoverPhotoId is { } id ? Photo(photos.GetValueOrDefault(id)) : null;

    /// <summary>A block as the site shows it; an image whose photo the library no longer has is left out.</summary>
    private static PublicArticleBlock? Block(ArticleBlock block, IReadOnlyDictionary<int, ContentMediaPhoto> photos)
    {
        if (block.Kind != ArticleBlockKind.Image)
            return new PublicArticleBlock(block.Kind, block.Text, block.Lead, block.Items ?? [], null, null);
        var photo = block.PhotoId is { } id ? Photo(photos.GetValueOrDefault(id)) : null;
        return photo is null ? null : new PublicArticleBlock(block.Kind, null, null, [], photo, block.Alt);
    }

    private static PublicArticlePhoto? Photo(ContentMediaPhoto? photo) =>
        photo is null ? null : new PublicArticlePhoto(photo.Url, photo.ThumbnailUrl, photo.Width, photo.Height);
}
