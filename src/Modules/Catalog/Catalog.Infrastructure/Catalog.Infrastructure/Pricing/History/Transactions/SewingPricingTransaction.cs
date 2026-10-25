using Catalog.Application.Contracts.Pricing;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Pricing.History.Transactions;

public sealed class SewingPricingTransaction(CatalogDbContext db) : ISewingPricingTransaction
{
    public async Task<IAsyncDisposable> BeginAsync(int productId, CancellationToken cancellationToken)
    {
        var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025004)", cancellationToken);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM catalog.\"Products\" WHERE \"Id\" = {productId} FOR UPDATE", cancellationToken);
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    public Task CommitAsync(CancellationToken cancellationToken)
        => db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
}
