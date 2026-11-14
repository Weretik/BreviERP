using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetContentArticle;

/// <summary>The editor of one article.</summary>
public sealed record GetContentArticleQuery(int Id) : IQuery<Result<ArticleResponse>>;
