namespace Reference.Application.Abstractions;

public interface IReferencePricingVersionReader
{
    Task<IReadOnlyList<ReferencePricingVersionValue>> ReadAsync(
        PricingReferenceRequest request, DateTimeOffset at, CancellationToken cancellationToken);
}

