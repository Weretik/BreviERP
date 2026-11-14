using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared;

public sealed record ArticleTextResponse(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ArticleBlock> Body);
