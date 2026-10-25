namespace Reference.Application.Contracts.Pricing;

public interface IReferencePricingUsageReader
{
    Task<bool> IsFabricVersionedAsync(int id, CancellationToken cancellationToken);
    Task<bool> IsAccessoryVersionedAsync(int id, CancellationToken cancellationToken);
    Task<bool> IsOperationVersionedAsync(int id, CancellationToken cancellationToken);
}
