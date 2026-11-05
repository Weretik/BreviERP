namespace Purchasing.Application.Abstractions;

/// <summary>A register write transaction (sync or manual review) under the module lock; disposing it without <see cref="CommitAsync"/> rolls everything back.</summary>
public interface IPurchaseWriteScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
