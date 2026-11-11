using Content.Application.Articles.ImportOldSite;
using Content.Domain.Articles;

namespace Content.Api.Articles;

/// <summary>
/// Body of <c>createContentArticle</c> and <c>saveContentArticle</c>; <c>Revision</c> is 0 for a new article,
/// <c>CoverPhotoId</c> is a photo of the «Фото» library or null.
/// </summary>
public sealed record SaveContentArticleRequest(
    int Revision,
    string? Slug,
    DateOnly PublishedOn,
    bool ShowOnSite,
    int? CoverPhotoId,
    ArticleTextInput? Uk,
    ArticleTextInput? Ru);

public sealed record ContentArticleVisibilityRequest(bool ShowOnSite);

/// <summary>Body of <c>importOldSiteArticles</c>; without <c>DryRun = false</c> nothing is written.</summary>
public sealed record ImportOldSiteArticlesRequest(
    IReadOnlyList<OldSiteArticle> Items, bool DryRun = true, bool ShowOnSite = false, bool Overwrite = false);
