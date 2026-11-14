using Media.Contracts.Storage;

using Production.Application.Abstractions;

namespace Production.Infrastructure.Adapters.Media;

/// <summary>Order files go to the same CDN as photos, straight through its storage: any file type, own folder.</summary>
internal sealed class ProductionFileStorage(ICdnStorage storage) : IProductionFileStorage
{
    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken) =>
        storage.UploadAsync(content, storageKey, contentType, cancellationToken);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) =>
        storage.DeleteAsync(storageKey, cancellationToken);
}
