using Accounting.Application.Abstractions;

namespace Accounting.Infrastructure.Files;

/// <summary>No bucket and no folder: the module works, only files are not accepted.</summary>
internal sealed class UnconfiguredAccountingFileStorage : IAccountingFileStorage
{
    public bool IsConfigured => false;

    public Task UploadAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Accounting file storage is not configured.");

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(null);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) => Task.CompletedTask;
}
