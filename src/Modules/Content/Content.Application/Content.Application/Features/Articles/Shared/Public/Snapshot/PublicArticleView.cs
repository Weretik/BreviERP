using BuildingBlocks.Domain.Identifiers.Products;
using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared.Public.Snapshot;

/// <summary>
/// What the public site needs of an article, copied out of the <see cref="Article"/> aggregate when the snapshot is
/// built: the cache and the public queries never hold the aggregate (CQRS, PLT-005 R-02).
/// </summary>
public sealed record PublicArticleView(
    string Slug,
    DateOnly PublishedOn,
    MediaFileId? CoverPhotoId,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<PublicArticleText> Texts,
    IReadOnlyList<int> PhotoIds)
{
    public PublicArticleText? TextIn(string language) => Texts.FirstOrDefault(x => x.Language == language);

    public static PublicArticleView From(Article article) =>
        new(article.Slug, article.PublishedOn, article.CoverPhotoId, article.UpdatedAtUtc,
            article.Texts.Select(x => new PublicArticleText(x.Language, x.Title, x.Excerpt, x.SeoTitle, x.SeoDescription,
                x.Body)).ToArray(),
            article.PhotoIds());
}

/// <summary>The text of an article in one language, as values.</summary>
public sealed record PublicArticleText(
    string Language,
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ArticleBlock> Body);
