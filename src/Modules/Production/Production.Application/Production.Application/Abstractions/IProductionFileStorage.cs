namespace Production.Application.Abstractions;

/// <summary>Stores order files on the CDN; implemented in the host over the Media storage.</summary>
public interface IProductionFileStorage
{
    /// <returns>The public URL of the stored file.</returns>
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
