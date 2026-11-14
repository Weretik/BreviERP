using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Public;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

public sealed class GetPublicArticleHandler(IPublicArticlesSource source)
    : IQueryHandler<GetPublicArticleQuery, Result<PublicArticle>>
{
    public async ValueTask<Result<PublicArticle>> Handle(GetPublicArticleQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var article = snapshot.Articles.FirstOrDefault(x => x.Slug == query.Slug);
        return article is null
            ? Result.NotFound("Статтю не знайдено.")
            : PublicArticleMapping.Page(article, query.Language, snapshot.Photos);
    }
}
