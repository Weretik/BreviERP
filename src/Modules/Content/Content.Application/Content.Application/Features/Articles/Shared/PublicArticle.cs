using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared;

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
