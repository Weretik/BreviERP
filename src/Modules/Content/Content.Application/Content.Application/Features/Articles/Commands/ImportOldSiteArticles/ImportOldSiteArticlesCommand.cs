using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

/// <summary>
/// One-time transfer of the old-site articles with SEO, texts and pictures (CNT-004 SC-06). An article is matched by
/// its address. <paramref name="DryRun"/> only counts. <paramref name="Overwrite"/> replaces articles that already
/// exist; without it they are kept. Pictures are downloaded from the old site into the «Фото» library, each once.
/// <paramref name="ShowOnSite"/> switches transferred articles on; it never switches one off. Every article is saved
/// on its own, so a repeated run continues where the previous one stopped.
/// </summary>
public sealed record ImportOldSiteArticlesCommand(
    bool DryRun,
    bool ShowOnSite,
    bool Overwrite,
    IReadOnlyList<OldSiteArticle> Items,
    string UpdatedBy) : ICommand<Result<OldSiteArticleImportResponse>>
{
    public const int MaxItems = 200;
}
