namespace Catalog.Application.Contracts.Pricing;

public interface ISewingPricingTransaction
{
    Task<IAsyncDisposable> BeginAsync(int productId, CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
}
