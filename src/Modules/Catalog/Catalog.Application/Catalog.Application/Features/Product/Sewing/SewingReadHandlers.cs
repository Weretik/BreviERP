namespace Catalog.Application.Features.Product.Sewing;

public sealed class ListSewingArticlesHandler(ISewingCompositionStore store)
    : IQueryHandler<ListSewingArticlesQuery, Result<SewingArticleList>>
{
    public async ValueTask<Result<SewingArticleList>> Handle(ListSewingArticlesQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId is <= 0 or > 1_000_000_000)
            return Result.Invalid(new ValidationError("productId", "Invalid Product ID."));
        var view = await store.ReadArticlesAsync(query.ProductId, cancellationToken);
        return view is null ? Result.NotFound() : Result.Success(view);
    }
}

public sealed class GetSewingCompositionHandler(ISewingCompositionStore store)
    : IQueryHandler<GetSewingCompositionQuery, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(GetSewingCompositionQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId is <= 0 or > 1_000_000_000 || query.ArticleId is <= 0)
            return Result.Invalid(new ValidationError("id", "Invalid Product or Article ID."));
        var view = await store.ReadCompositionAsync(query.ProductId, query.ArticleId, cancellationToken);
        return view is null ? Result.NotFound() : Result.Success(view);
    }
}
