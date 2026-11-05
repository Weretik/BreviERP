using Microsoft.EntityFrameworkCore.Storage;
using Purchasing.Application.Abstractions;

namespace Purchasing.Infrastructure.Persistence;

internal sealed class PurchaseWriteScope(IDbContextTransaction transaction) : IPurchaseWriteScope
{
    private bool disposed;

    public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        await transaction.DisposeAsync();
    }
}
