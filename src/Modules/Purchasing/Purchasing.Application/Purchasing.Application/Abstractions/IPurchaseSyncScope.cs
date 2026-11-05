namespace Purchasing.Application.Abstractions;

/// <summary>The sync transaction; disposing it without <see cref="CommitAsync"/> rolls everything back.</summary>
public interface IPurchaseSyncScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
