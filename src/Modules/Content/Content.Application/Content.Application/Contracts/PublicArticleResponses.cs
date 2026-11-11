using Content.Domain.Articles;

namespace Content.Application.Contracts;

/// <summary>A photo of an article on the public site: the CDN address, its small copy and its size when known.</summary>
public sealed record PublicArticlePhoto(string Url, string? ThumbnailUrl, int? Width, int? Height);

/// <summary>
/// An article in the list of the site, in <see cref="Language"/>: the asked language, or Ukrainian when the article
/// has no Russian text.
/// </summary>
public sealed record PublicArticleCard(
    string Slug,
    DateOnly PublishedOn,
    string Language,
    string Title,
    string? Excerpt,
    PublicArticlePhoto? Cover,
    bool HasRussian,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Every article on the site, the newest first.</summary>
public sealed record PublicArticleList(IReadOnlyList<PublicArticleCard> Items);

/// <summary>One block of an article text; an image block carries its photo and alt text.</summary>
public sealed record PublicArticleBlock(
    ArticleBlockKind Kind,
    string? Text,
    string? Lead,
    IReadOnlyList<string> Items,
    PublicArticlePhoto? Photo,
    string? Alt);

/// <summary>An article page of the site in <see cref="Language"/> (see <see cref="PublicArticleCard"/>).</summary>
public sealed record PublicArticle(
    string Slug,
    DateOnly PublishedOn,
    string Language,
    bool HasRussian,
    string Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    PublicArticlePhoto? Cover,
    IReadOnlyList<PublicArticleBlock> Body,
    DateTimeOffset UpdatedAtUtc);
