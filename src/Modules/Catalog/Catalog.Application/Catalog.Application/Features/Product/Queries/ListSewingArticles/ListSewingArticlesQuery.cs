namespace Catalog.Application.Features.Product.Queries.ListSewingArticles;

public sealed record ListSewingArticlesQuery(int ProductId) : IQuery<Result<SewingArticleList>>;

