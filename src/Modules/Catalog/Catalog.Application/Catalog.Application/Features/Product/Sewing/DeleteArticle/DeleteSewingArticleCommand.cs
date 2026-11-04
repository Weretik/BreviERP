namespace Catalog.Application.Features.Product.Sewing;

public sealed record DeleteSewingArticleCommand(int ProductId, long ArticleId, long ExpectedVersion) : ICommand<Result>;
