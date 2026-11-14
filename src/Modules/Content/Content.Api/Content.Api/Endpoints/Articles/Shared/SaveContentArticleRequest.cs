using Content.Domain.Articles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

namespace Content.Api.Endpoints.Articles.Shared;

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
