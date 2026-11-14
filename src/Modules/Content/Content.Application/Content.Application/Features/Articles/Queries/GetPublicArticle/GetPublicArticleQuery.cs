using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

/// <summary>An article page of the site by its address. Unknown and hidden articles are both «not found».</summary>
public sealed record GetPublicArticleQuery(string Slug, string Language) : IQuery<Result<PublicArticle>>;
