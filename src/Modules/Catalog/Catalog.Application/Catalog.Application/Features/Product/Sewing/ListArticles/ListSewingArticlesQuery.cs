namespace Catalog.Application.Features.Product.Sewing;

public sealed record ListSewingArticlesQuery(int ProductId) : IQuery<Result<SewingArticleList>>;

