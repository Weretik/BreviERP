using Content.Domain.Articles;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

namespace Content.Api.Endpoints.Articles.SetContentArticleVisibility;

public sealed record ContentArticleVisibilityRequest(bool ShowOnSite);
