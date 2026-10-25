namespace Reference.Application.Contracts.Pricing;

public interface IReferencePricingVersionReader
{
    Task<IReadOnlyList<ReferencePricingVersionValue>> ReadAsync(
        PricingReferenceRequest request, DateTimeOffset at, CancellationToken cancellationToken);
}

