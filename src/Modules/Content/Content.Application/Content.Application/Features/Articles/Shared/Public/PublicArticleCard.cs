using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared.Public;

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
