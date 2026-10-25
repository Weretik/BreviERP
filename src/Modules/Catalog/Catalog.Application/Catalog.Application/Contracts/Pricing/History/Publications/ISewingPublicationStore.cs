namespace Catalog.Application.Contracts.Pricing;

public interface ISewingPublicationStore
{
    Task<PublishedSewingPrice?> FindPublicationAsync(Guid calculationId, CancellationToken cancellationToken);
    Task<PublishedSewingPrice?> ReadCurrentAsync(int productId, long articleId, CancellationToken cancellationToken);
    Task PublishAsync(PublishedSewingPrice publication, CancellationToken cancellationToken);
}
