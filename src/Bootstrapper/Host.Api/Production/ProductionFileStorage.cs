using Media.Application.Abstractions;
using Production.Application.Abstractions;

namespace Host.Api.Production;

/// <summary>Order files go to the same CDN as photos, straight through its storage: any file type, own folder.</summary>
internal sealed class ProductionFileStorage(IMediaStorage storage) : IProductionFileStorage
{
    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken) =>
        storage.UploadAsync(content, storageKey, contentType, cancellationToken);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) =>
        storage.DeleteAsync(storageKey, cancellationToken);
}
