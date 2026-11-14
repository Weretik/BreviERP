using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Sewing;
namespace Catalog.Application.Features.Product.Queries.ListSewingArticles;

public sealed class ListSewingArticlesHandler(ISewingCompositionReadStore store)
    : IQueryHandler<ListSewingArticlesQuery, Result<SewingArticleList>>
{
    public async ValueTask<Result<SewingArticleList>> Handle(ListSewingArticlesQuery query, CancellationToken cancellationToken)
    {
        var view = await store.ReadArticlesAsync(query.ProductId, cancellationToken);
        return view is null ? Result.NotFound() : Result.Success(view);
    }
}
