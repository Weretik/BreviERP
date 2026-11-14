namespace Reference.Application.Abstractions.Pricing;

public interface IReferencePricingVersionReader
{
    Task<IReadOnlyList<ReferencePricingVersionValue>> ReadAsync(
        PricingReferenceRequest request, DateTimeOffset at, CancellationToken cancellationToken);
}

