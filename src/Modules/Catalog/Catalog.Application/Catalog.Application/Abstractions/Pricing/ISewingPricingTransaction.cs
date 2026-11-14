namespace Catalog.Application.Abstractions.Pricing;

public interface ISewingPricingTransaction
{
    Task<IAsyncDisposable> BeginAsync(int productId, CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
}
