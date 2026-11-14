using Content.Domain.Articles;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetContentArticles;

/// <summary>Every article, the newest first.</summary>
public sealed record ArticleList(IReadOnlyList<ArticleRow> Items);
