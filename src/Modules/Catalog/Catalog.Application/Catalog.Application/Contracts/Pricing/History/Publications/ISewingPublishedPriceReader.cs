namespace Catalog.Application.Contracts.Pricing;

public interface ISewingPublishedPriceReader
{
    Task<IReadOnlyList<SewingPublishedProductPrices>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
