using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;

namespace Purchasing.Infrastructure.Integrations.Dropbox;

/// <summary>
/// Downloads a shared Dropbox file (<c>dl=1</c>) for recognition: only safe links, at most 20 MB, only PDF and pictures.
/// Dropbox answers with a redirect to its content host; the file is read in memory and not stored.
/// </summary>
internal sealed class DropboxFileDownloader(HttpClient http) : IPurchaseFileDownloader
{
    public const int MaxBytes = 20 * 1024 * 1024;

    private static readonly Dictionary<string, string> MediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
    };

    public async Task<PurchaseFileContent> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        if (!FileLinkRules.IsSafeDropbox(url)) throw new PurchaseFileUnavailableException("Посилання не з Dropbox.");
        var uri = new Uri(url);
        var fileName = Uri.UnescapeDataString(uri.Segments[^1]);
        var extension = Path.GetExtension(fileName);
        if (!MediaTypes.TryGetValue(extension, out var mediaType))
            throw new PurchaseFileUnavailableException(
                $"Файл «{extension}» ШІ не читає (лише PDF і фото). Завантажте JSON вручну.", unsupported: true);

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync(DirectLink(uri), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new PurchaseFileUnavailableException("Dropbox не відповідає. Спробуйте пізніше.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new PurchaseFileUnavailableException($"Dropbox не віддав файл (HTTP {(int)response.StatusCode}); можливо, посилання вже недійсне.");
            if (response.Content.Headers.ContentType?.MediaType is "text/html")
                throw new PurchaseFileUnavailableException("Dropbox повернув сторінку замість файлу; можливо, посилання вже недійсне.");
            if (response.Content.Headers.ContentLength > MaxBytes)
                throw new PurchaseFileUnavailableException("Файл більший за 20 МБ.");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memory = new MemoryStream();
            var buffer = new byte[81_920];
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                if (memory.Length + read > MaxBytes) throw new PurchaseFileUnavailableException("Файл більший за 20 МБ.");
                memory.Write(buffer, 0, read);
            }

            if (memory.Length == 0) throw new PurchaseFileUnavailableException("Dropbox віддав порожній файл.");
            return new PurchaseFileContent(fileName, mediaType, memory.ToArray());
        }
    }

    /// <summary>The same shared link with <c>dl=1</c>, so Dropbox sends the file instead of its preview page.</summary>
    internal static Uri DirectLink(Uri uri)
    {
        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !x.StartsWith("dl=", StringComparison.OrdinalIgnoreCase) && !x.StartsWith("raw=", StringComparison.OrdinalIgnoreCase))
            .Append("dl=1");
        return new UriBuilder(uri) { Query = string.Join('&', query) }.Uri;
    }
}
