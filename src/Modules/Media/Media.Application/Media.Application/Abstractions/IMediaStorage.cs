namespace Media.Application.Abstractions;

/// <summary>The CDN that keeps photo files.</summary>
public interface IMediaStorage
{
    string ProviderName { get; }

    /// <returns>The public URL of the stored file.</returns>
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
