namespace Catalog.Application.Abstractions.Pricing;

public interface IPricingReferenceVersionReader
{
    Task<IReadOnlyList<PricingReferenceValue>> ReadAsync(
        IReadOnlyCollection<string> referenceKeys, DateTimeOffset at, CancellationToken cancellationToken);
}

