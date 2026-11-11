using Crm.Application.SiteRequests.Abstractions;
using Media.Application.Abstractions;

namespace Host.Api.Crm;

/// <summary>Files of site requests go to the CDN next to the other files (LEAD-002), not into the photo library.</summary>
internal sealed class SiteRequestFileStorage(IMediaStorage storage) : ISiteRequestFileStorage
{
    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken) =>
        storage.UploadAsync(content, storageKey, contentType, cancellationToken);
}
