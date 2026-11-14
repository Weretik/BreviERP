using Media.Application.Abstractions;
using Media.Contracts.Storage;

namespace Media.Infrastructure.Contracts.Storage;

/// <summary>The CDN storage of the photo library, for files that are not library photos.</summary>
internal sealed class CdnStorage(IMediaStorage storage, IConfiguration configuration) : ICdnStorage
{
    private string PublicBaseUrl => configuration["AdmToolsStorage:PublicBaseUrl"] ?? string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(configuration["AdmToolsStorage:Login"]) && !string.IsNullOrWhiteSpace(PublicBaseUrl);

    public string? PublicUrl(string storageKey) =>
        string.IsNullOrWhiteSpace(PublicBaseUrl) ? null : $"{PublicBaseUrl.TrimEnd('/')}{storageKey}";

    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken) =>
        storage.UploadAsync(content, storageKey, contentType, cancellationToken);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken) =>
        storage.DeleteAsync(storageKey, cancellationToken);
}
