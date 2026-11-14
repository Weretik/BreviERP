using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.SetContentArticleVisibility;

/// <summary>«Показувати на сайті» from the list (CNT-004 SC-03).</summary>
public sealed record SetContentArticleVisibilityCommand(int Id, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<ArticleVisibilityResponse>>;
