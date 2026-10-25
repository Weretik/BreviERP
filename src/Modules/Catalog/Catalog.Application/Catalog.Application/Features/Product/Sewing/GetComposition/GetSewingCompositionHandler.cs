namespace Catalog.Application.Features.Product.Sewing;

public sealed class GetSewingCompositionHandler(ISewingCompositionReadStore readStore)
    : IQueryHandler<GetSewingCompositionQuery, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(GetSewingCompositionQuery query, CancellationToken cancellationToken)
    {
        var errors = GetSewingCompositionQueryValidator.Validate(query);
        if (errors.Count > 0)
            return Result.Invalid(errors);

        var snapshot = await readStore.ReadCompositionAsync(query.ProductId, query.ArticleId, cancellationToken);
        return snapshot is null
            ? Result.NotFound()
            : Result.Success(SewingCompositionReadAssembler.Assemble(snapshot));
    }
}
