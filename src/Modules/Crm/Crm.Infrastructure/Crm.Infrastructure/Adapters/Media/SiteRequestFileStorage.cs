
using Media.Contracts.Storage;
using Crm.Application.Abstractions.SiteRequests;

namespace Crm.Infrastructure.Adapters.Media;

/// <summary>Files of site requests go to the CDN next to the other files (LEAD-002), not into the photo library.</summary>
internal sealed class SiteRequestFileStorage(ICdnStorage storage) : ISiteRequestFileStorage
{
    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken) =>
        storage.UploadAsync(content, storageKey, contentType, cancellationToken);
}
