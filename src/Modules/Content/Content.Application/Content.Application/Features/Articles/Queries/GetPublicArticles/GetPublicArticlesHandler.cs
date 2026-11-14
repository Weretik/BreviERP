using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

public sealed class GetPublicArticlesHandler(IPublicArticlesSource source)
    : IQueryHandler<GetPublicArticlesQuery, Result<PublicArticleList>>
{
    public async ValueTask<Result<PublicArticleList>> Handle(GetPublicArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return new PublicArticleList(snapshot.Articles
            .Select(x => PublicArticleMapping.Card(x, query.Language, snapshot.Photos)).ToArray());
    }
}
