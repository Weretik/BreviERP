using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Integrations.Pdf;

/// <summary>A photo or logo for the PDF; SVG is kept as text.</summary>
internal sealed record QuotePdfImage(byte[]? Bitmap, string? Svg);

/// <summary>
/// Downloads the КП photos and logo (public https links of the photo library and site). A photo that cannot be
/// read is left out, so a broken link never stops the PDF.
/// </summary>
internal sealed class QuotePdfImageLoader(HttpClient http, ILogger<QuotePdfImageLoader> logger)
{
    private const int MaxBytes = 8 * 1024 * 1024;

    public async Task<IReadOnlyDictionary<string, QuotePdfImage>> LoadAsync(IEnumerable<string?> urls,
        CancellationToken cancellationToken)
    {
        var distinct = urls.Where(x => x is not null && IsHttps(x)).Select(x => x!).Distinct().ToArray();
        var loaded = await Task.WhenAll(distinct.Select(url => LoadOneAsync(url, cancellationToken)));
        return distinct.Zip(loaded).Where(x => x.Second is not null)
            .ToDictionary(x => x.First, x => x.Second!, StringComparer.Ordinal);
    }

    private async Task<QuotePdfImage?> LoadOneAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaxBytes) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (bytes.Length is 0 or > MaxBytes) return null;
            var isSvg = response.Content.Headers.ContentType?.MediaType == "image/svg+xml" ||
                url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
            var image = isSvg ? new QuotePdfImage(null, System.Text.Encoding.UTF8.GetString(bytes)) : new QuotePdfImage(bytes, null);
            if (CanDraw(image)) return image;
            logger.LogWarning("КП PDF: photo {Url} is not an image the PDF can draw", url);
            return null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "КП PDF: photo {Url} is unavailable", url);
            return null;
        }
    }

    /// <summary>
    /// A link may answer with a page or a format the PDF cannot decode (HEIC, a broken file); such a photo is left
    /// out instead of failing the whole PDF.
    /// </summary>
    internal static bool CanDraw(QuotePdfImage image)
    {
        try
        {
            if (image.Svg is not null) QuestPDF.Infrastructure.SvgImage.FromText(image.Svg);
            else QuestPDF.Infrastructure.Image.FromBinaryData(image.Bitmap!);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsHttps(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
