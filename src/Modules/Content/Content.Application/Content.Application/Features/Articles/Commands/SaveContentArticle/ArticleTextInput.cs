using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Commands.SaveContentArticle;

/// <summary>What staff type for one language of an article: the body of the save request and of the old-site import.</summary>
public sealed record ArticleTextInput(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ArticleBlock>? Body)
{
    public ArticleTextValues ToValues() => new(Title, Excerpt, SeoTitle, SeoDescription, Body);
}
