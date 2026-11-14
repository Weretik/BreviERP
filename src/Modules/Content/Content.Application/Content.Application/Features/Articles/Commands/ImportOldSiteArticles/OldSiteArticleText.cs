using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

public sealed record OldSiteArticleText(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<OldSiteArticleBlock>? Body)
{
    /// <summary>The texts as an article keeps them; <paramref name="photo"/> gives the library photo of a picture address.</summary>
    public ArticleTextInput ToInput(Func<string, int?> photo) => new(Title, Excerpt, SeoTitle, SeoDescription,
        (Body ?? []).Where(block => block is not null).Select(block => new ArticleBlock(block.Kind, block.Text,
            block.Lead, block.Items, block.Kind == ArticleBlockKind.Image ? photo(block.ImageUrl ?? string.Empty) : null,
            block.Alt)).ToArray());
}
