namespace Identity.Application.Abstractions.Profile;

/// <summary>
/// Where processed avatar files live. Host.Api stores them on the Brevi CDN next to the photo library;
/// without it, avatars are switched off.
/// </summary>
public interface IAvatarStorage
{
    bool IsConfigured { get; }

    /// <summary>Saves a WebP file under a key relative to the avatars folder.</summary>
    Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>The public URL, built at read time so a host change needs no data change.</summary>
    string? BuildUrl(string? key);
}
