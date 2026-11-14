namespace Catalog.Application.Features.Product.Sewing;

public sealed class GetSewingCompositionHandler(ISewingCompositionReadStore readStore)
    : IQueryHandler<GetSewingCompositionQuery, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(GetSewingCompositionQuery query, CancellationToken cancellationToken)
    {
        var snapshot = await readStore.ReadCompositionAsync(query.ProductId, query.ArticleId, cancellationToken);
        return snapshot is null
            ? Result.NotFound()
            : Result.Success(SewingCompositionReadAssembler.Assemble(snapshot));
    }
}
