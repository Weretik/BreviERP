using Identity.Application.Services;
using Media.Application.Abstractions;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>
/// Keeps staff avatars on the Brevi CDN through the Media module's storage, in their own folder.
/// Avatars are not photo library files and never show in «Фото».
/// </summary>
internal sealed class IdentityAvatarStorage(IMediaStorage storage, IConfiguration configuration) : IAvatarStorage
{
    private const string Folder = "/avatars";

    private string PublicBaseUrl => configuration["AdmToolsStorage:PublicBaseUrl"] ?? string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(configuration["AdmToolsStorage:Login"]) && !string.IsNullOrWhiteSpace(PublicBaseUrl);

    public async Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(content, writable: false);
        await storage.UploadAsync(stream, StorageKey(key), "image/webp", cancellationToken);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
        storage.DeleteAsync(StorageKey(key), cancellationToken);

    public string? BuildUrl(string? key) =>
        string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(PublicBaseUrl)
            ? null
            : $"{PublicBaseUrl.TrimEnd('/')}{StorageKey(key)}";

    private static string StorageKey(string key) => $"{Folder}/{key.TrimStart('/')}";
}
