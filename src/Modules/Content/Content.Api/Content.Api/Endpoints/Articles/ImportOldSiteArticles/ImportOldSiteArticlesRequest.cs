using Content.Domain.Articles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

namespace Content.Api.Endpoints.Articles.ImportOldSiteArticles;

/// <summary>Body of <c>importOldSiteArticles</c>; without <c>DryRun = false</c> nothing is written.</summary>
public sealed record ImportOldSiteArticlesRequest(
    IReadOnlyList<OldSiteArticle> Items, bool DryRun = true, bool ShowOnSite = false, bool Overwrite = false);
