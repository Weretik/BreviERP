namespace Media.Contracts.Storage;

/// <summary>
/// Raw files on the Brevi CDN outside the «Фото» library (order files, site request files, avatars).
/// </summary>
public interface ICdnStorage
{
    /// <summary>False when the CDN sign-in or public address is not configured on this server.</summary>
    bool IsConfigured { get; }

    /// <summary>The public address of a stored key, or null when the CDN is not configured.</summary>
    string? PublicUrl(string storageKey);

    /// <returns>The public URL of the stored file.</returns>
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
