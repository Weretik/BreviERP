namespace Catalog.Application.Abstractions.Pricing.Publications;

public interface ISewingPublicationStore
{
    Task<PublishedSewingPrice?> FindPublicationAsync(Guid calculationId, CancellationToken cancellationToken);
    Task<PublishedSewingPrice?> ReadCurrentAsync(int productId, long articleId, CancellationToken cancellationToken);
    Task PublishAsync(PublishedSewingPrice publication, CancellationToken cancellationToken);
}
