namespace Catalog.Application.Features.Product.Sewing;

public sealed record UpdateSewingArticleCommand(int ProductId, long ArticleId, UpdateSewingArticleRequest Request)
    : ICommand<Result<SewingArticleView>>;
