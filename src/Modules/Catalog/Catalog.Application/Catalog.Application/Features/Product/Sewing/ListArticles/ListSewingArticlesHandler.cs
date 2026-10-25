namespace Catalog.Application.Features.Product.Sewing;

public sealed class ListSewingArticlesHandler(ISewingCompositionReadStore store)
    : IQueryHandler<ListSewingArticlesQuery, Result<SewingArticleList>>
{
    public async ValueTask<Result<SewingArticleList>> Handle(ListSewingArticlesQuery query, CancellationToken cancellationToken)
    {
        var errors = ListSewingArticlesQueryValidator.Validate(query);
        if (errors.Count > 0)
            return Result.Invalid(errors);

        var view = await store.ReadArticlesAsync(query.ProductId, cancellationToken);
        return view is null ? Result.NotFound() : Result.Success(view);
    }
}
