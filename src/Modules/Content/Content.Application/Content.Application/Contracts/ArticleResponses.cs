using Content.Domain.Articles;

namespace Content.Application.Contracts;

/// <summary>
/// A row of «Контент → Статті». <paramref name="HasSeo"/> is true when both languages have a SEO title and
/// description. <paramref name="CoverUrl"/> is the small copy of the cover, or the photo itself without one.
/// </summary>
public sealed record ArticleRow(
    int Id,
    string Slug,
    DateOnly PublishedOn,
    bool ShowOnSite,
    string? TitleUk,
    string? TitleRu,
    bool HasUkrainianText,
    bool HasRussianText,
    bool HasSeo,
    string? CoverUrl,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Every article, the newest first.</summary>
public sealed record ArticleList(IReadOnlyList<ArticleRow> Items);

public sealed record ArticleTextResponse(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ArticleBlock> Body);

/// <summary>A photo of the «Фото» library that the article uses.</summary>
public sealed record ArticlePhoto(int Id, string Url, string? ThumbnailUrl, int? Width, int? Height);

/// <summary>
/// The editor of one article. <paramref name="Photos"/> are the cover and the photos of the bodies that the library
/// still has, so the editor can show them; a photo deleted from the library is not in the list.
/// </summary>
public sealed record ArticleResponse(
    ArticleRow Article,
    int Revision,
    string UpdatedBy,
    int? CoverPhotoId,
    ArticleTextResponse Uk,
    ArticleTextResponse Ru,
    IReadOnlyList<ArticlePhoto> Photos);

public sealed record ArticleVisibilityResponse(int Id, bool ShowOnSite, int Revision);
