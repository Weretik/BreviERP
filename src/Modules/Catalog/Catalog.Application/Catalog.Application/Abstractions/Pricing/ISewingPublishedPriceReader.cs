namespace Catalog.Application.Abstractions.Pricing;

public interface ISewingPublishedPriceReader
{
    Task<IReadOnlyList<SewingPublishedProductPrices>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
