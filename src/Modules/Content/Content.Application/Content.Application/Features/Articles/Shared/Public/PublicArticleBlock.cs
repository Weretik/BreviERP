using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Shared.Public;

/// <summary>One block of an article text; an image block carries its photo and alt text.</summary>
public sealed record PublicArticleBlock(
    ArticleBlockKind Kind,
    string? Text,
    string? Lead,
    IReadOnlyList<string> Items,
    PublicArticlePhoto? Photo,
    string? Alt);
