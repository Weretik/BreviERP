namespace Catalog.Application.Abstractions.Pricing.Publications;

public interface ISewingPublishedPriceReader
{
    Task<IReadOnlyList<SewingPublishedProductPrices>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
