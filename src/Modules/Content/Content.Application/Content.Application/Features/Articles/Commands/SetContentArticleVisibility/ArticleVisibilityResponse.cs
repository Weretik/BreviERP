using Content.Domain.Articles;

namespace Content.Application.Features.Articles.Commands.SetContentArticleVisibility;

public sealed record ArticleVisibilityResponse(int Id, bool ShowOnSite, int Revision);
