using Media.Contracts.Storage;
using Identity.Application.Abstractions;

namespace Identity.Infrastructure.Adapters.Media;

/// <summary>
/// Keeps staff avatars on the Brevi CDN through the Media module's storage, in their own folder.
/// Avatars are not photo library files and never show in «Фото».
/// </summary>
internal sealed class CdnAvatarStorage(ICdnStorage storage) : IAvatarStorage
{
    private const string Folder = "/avatars";

    public bool IsConfigured => storage.IsConfigured;

    public async Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(content, writable: false);
        await storage.UploadAsync(stream, StorageKey(key), "image/webp", cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        storage.DeleteAsync(StorageKey(key), cancellationToken);

    public string? BuildUrl(string? key) =>
        string.IsNullOrWhiteSpace(key) ? null : storage.PublicUrl(StorageKey(key));

    private static string StorageKey(string key) => $"{Folder}/{key.TrimStart('/')}";
}
