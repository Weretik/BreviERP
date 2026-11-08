using Accounting.Application.Abstractions;

namespace Accounting.Infrastructure.Files;

/// <summary>A folder on the developer's machine; never used on Cloud Run.</summary>
internal sealed class LocalAccountingFileStorage(string root) : IAccountingFileStorage
{
    private readonly string root = Path.GetFullPath(root);

    public bool IsConfigured => true;

    public async Task UploadAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = PathOf(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        var path = PathOf(storageKey);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        File.Delete(PathOf(storageKey));
        return Task.CompletedTask;
    }

    /// <summary>Keys are made by the module, but a path outside the folder is refused anyway.</summary>
    private string PathOf(string storageKey)
    {
        var path = Path.GetFullPath(Path.Combine(root, storageKey));
        return path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? path
            : throw new InvalidOperationException("Storage key points outside the folder.");
    }
}
