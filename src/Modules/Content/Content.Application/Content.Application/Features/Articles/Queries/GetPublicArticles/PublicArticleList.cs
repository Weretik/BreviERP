using Content.Domain.Articles;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Public;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

/// <summary>Every article on the site, the newest first.</summary>
public sealed record PublicArticleList(IReadOnlyList<PublicArticleCard> Items);
