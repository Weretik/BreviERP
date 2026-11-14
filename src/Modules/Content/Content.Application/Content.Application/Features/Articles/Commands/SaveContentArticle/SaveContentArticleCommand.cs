using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Articles.Commands.SaveContentArticle;

/// <summary>
/// Creates an article (<paramref name="Id"/> is null, <paramref name="Revision"/> 0) or replaces it as a whole
/// (CNT-004 R-03). An empty <paramref name="Slug"/> is built from the Ukrainian title. The cover and the photos of the
/// bodies must be in the «Фото» library.
/// </summary>
public sealed record SaveContentArticleCommand(
    int? Id,
    int Revision,
    string? Slug,
    DateOnly PublishedOn,
    bool ShowOnSite,
    int? CoverPhotoId,
    ArticleTextInput? Uk,
    ArticleTextInput? Ru,
    string UpdatedBy) : ICommand<Result<ArticleResponse>>;
