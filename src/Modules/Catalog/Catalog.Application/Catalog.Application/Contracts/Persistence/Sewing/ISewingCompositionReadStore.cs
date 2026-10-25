namespace Catalog.Application.Features.Product.Sewing;

public interface ISewingCompositionReadStore
{
    Task<SewingArticleList?> ReadArticlesAsync(int productId, CancellationToken cancellationToken);
    Task<SewingCompositionReadSnapshot?> ReadCompositionAsync(int productId, long? articleId, CancellationToken cancellationToken);
}
