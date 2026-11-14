using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

/// <summary>Every article on the site, the newest first, in the asked language (Ukrainian when it has no Russian text).</summary>
public sealed record GetPublicArticlesQuery(string Language) : IQuery<Result<PublicArticleList>>;
