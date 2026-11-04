namespace Media.Application.Abstractions;

/// <summary>The CDN that keeps photo files.</summary>
public interface IMediaStorage
{
    string ProviderName { get; }

    /// <returns>The public URL of the stored file.</returns>
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);

    /// <summary>Reads a stored file back by its public URL, for the preview backfill.</summary>
    Task<byte[]> DownloadAsync(string publicUrl, CancellationToken cancellationToken);
}
