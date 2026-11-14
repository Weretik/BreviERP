namespace Catalog.Application.Features.Product.Commands.DeleteSewingArticle;

public sealed record DeleteSewingArticleCommand(int ProductId, long ArticleId, long ExpectedVersion) : ICommand<Result>;
