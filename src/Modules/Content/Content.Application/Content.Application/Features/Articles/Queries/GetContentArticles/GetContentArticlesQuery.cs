using Content.Application.Abstractions;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetContentArticles;

/// <summary>Every article of «Контент → Статті», the newest first (CNT-004 SC-01).</summary>
public sealed record GetContentArticlesQuery : IQuery<Result<ArticleList>>;
