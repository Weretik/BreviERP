using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Views;
namespace Catalog.Application.Features.Product.Commands.CreateSewingArticle;

public sealed record CreateSewingArticleCommand(int ProductId, CreateSewingArticleRequest Request) : ICommand<Result<SewingArticleView>>;

