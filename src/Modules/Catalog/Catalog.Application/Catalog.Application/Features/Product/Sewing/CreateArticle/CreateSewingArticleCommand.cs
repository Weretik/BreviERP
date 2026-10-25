namespace Catalog.Application.Features.Product.Sewing;

public sealed record CreateSewingArticleCommand(int ProductId, CreateSewingArticleRequest Request) : ICommand<Result<SewingArticleView>>;

