using Mediator;
using Media.Application.Abstractions;
using Media.Application.Files.GetFilesBySourceKeys;
using Media.Application.Files.UploadFile;
using Media.Application.Folders.EnsureFolderPath;
using Media.Contracts.Imports;

namespace Media.Infrastructure.Contracts.Imports;

/// <summary>
/// Downloads pictures for photo imports (Catalog CAT-016, Content articles) and adds them to the library through its
/// commands, with the source key that makes a repeated import skip them.
/// </summary>
internal sealed class MediaUrlImporter(HttpClient http, ISender sender, MediaUploadSettings settings)
    : IMediaUrlImporter
{
    private readonly Dictionary<string, int> _folders = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyDictionary<string, int>> FindImportedAsync(
        IReadOnlyCollection<string> sourceKeys, CancellationToken cancellationToken)
    {
        if (sourceKeys.Count == 0) return new Dictionary<string, int>();
        var result = await sender.Send(new GetMediaFilesBySourceKeysQuery(sourceKeys), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Imported library photos could not be read.");
        return result.Value;
    }

    public async Task<MediaUrlImport> ImportAsync(
        Uri url, string sourceKey, IReadOnlyList<string> folderPath, CancellationToken cancellationToken)
    {
        var contentType = ContentTypeOf(url.AbsolutePath);
        if (contentType is null) return Failed("формат не JPG, PNG або WebP");

        byte[] content;
        try
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return Failed($"сайт відповів {(int)response.StatusCode}");
            if (response.Content.Headers.ContentLength > settings.MaxFileSizeBytes) return Failed("файл завеликий");
            content = await ReadLimitedAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken);
            if (content.Length == 0) return Failed("файл порожній");
            if (content.Length > settings.MaxFileSizeBytes) return Failed("файл завеликий");
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Failed("сайт не відповів");
        }

        var folderId = await FolderAsync(folderPath, cancellationToken);
        if (folderId is null) return Failed("папку в медіатеці не створено");

        var name = Uri.UnescapeDataString(url.AbsolutePath[(url.AbsolutePath.LastIndexOf('/') + 1)..]);
        using var stream = new MemoryStream(content, writable: false);
        var uploaded = await sender.Send(new UploadMediaFileCommand(stream, name, contentType, content.Length, folderId, sourceKey),
            cancellationToken);
        if (uploaded.IsSuccess) return new MediaUrlImport(uploaded.Value.Id, null);

        // Another run imported it meanwhile: use that photo.
        var existing = await FindImportedAsync([sourceKey], cancellationToken);
        if (existing.TryGetValue(sourceKey, out var id)) return new MediaUrlImport(id, null);
        var reason = uploaded.ValidationErrors.Select(x => x.ErrorMessage).Concat(uploaded.Errors).FirstOrDefault();
        return Failed(reason ?? "медіатека не прийняла файл");
    }

    private async Task<int?> FolderAsync(IReadOnlyList<string> path, CancellationToken cancellationToken)
    {
        var key = string.Join('/', path);
        if (_folders.TryGetValue(key, out var cached)) return cached;
        var result = await sender.Send(new EnsureMediaFolderPathCommand(path), cancellationToken);
        if (!result.IsSuccess) return null;
        _folders[key] = result.Value;
        return result.Value;
    }

    private async Task<byte[]> ReadLimitedAsync(Stream source, CancellationToken cancellationToken)
    {
        await using (source)
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await source.ReadAsync(chunk, cancellationToken)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > settings.MaxFileSizeBytes) break;
            }

            return buffer.ToArray();
        }
    }

    private static string? ContentTypeOf(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => null
    };

    private static MediaUrlImport Failed(string reason) => new(null, reason);
}
