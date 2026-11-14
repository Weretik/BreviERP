using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Features.Product.Commands.UpdateSewingArticle;

public sealed record UpdateSewingArticleCommand(int ProductId, long ArticleId, UpdateSewingArticleRequest Request)
    : ICommand<Result<SewingArticleView>>;
