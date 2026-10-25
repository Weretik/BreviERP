namespace Catalog.Application.Contracts.Pricing;

public interface IPricingReferenceVersionReader
{
    Task<IReadOnlyList<PricingReferenceValue>> ReadAsync(
        IReadOnlyCollection<string> referenceKeys, DateTimeOffset at, CancellationToken cancellationToken);
}

