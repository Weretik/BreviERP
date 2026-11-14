using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared;

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
