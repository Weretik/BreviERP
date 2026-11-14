using Catalog.Application.Features.Product.Queries.ListSewingArticles;
namespace Catalog.Application.Abstractions;

public interface ISewingCompositionReadStore
{
    Task<SewingArticleList?> ReadArticlesAsync(int productId, CancellationToken cancellationToken);
    Task<SewingCompositionReadSnapshot?> ReadCompositionAsync(int productId, long? articleId, CancellationToken cancellationToken);
}
