using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Queries.GetSewingComposition;

public sealed record GetSewingCompositionQuery(int ProductId, long? ArticleId) : IQuery<Result<SewingComposition>>;

