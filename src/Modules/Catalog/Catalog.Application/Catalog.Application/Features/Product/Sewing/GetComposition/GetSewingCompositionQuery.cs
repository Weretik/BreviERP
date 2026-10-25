namespace Catalog.Application.Features.Product.Sewing;

public sealed record GetSewingCompositionQuery(int ProductId, long? ArticleId) : IQuery<Result<SewingComposition>>;

