namespace Catalog.Api.Validation;

internal static class ImageFileSignatureValidator
{
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] WebpRiffSignature = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] WebpSignature = [0x57, 0x45, 0x42, 0x50];

    public static async Task<bool> IsValidAsync(Stream stream, string contentType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = new byte[12];
        var bytesRead = await ReadAtLeastAsync(stream, buffer, cancellationToken);

        if (stream.CanSeek)
            stream.Position = 0;

        return contentType switch
        {
            "image/jpeg" => StartsWith(buffer, bytesRead, JpegSignature),
            "image/png" => StartsWith(buffer, bytesRead, PngSignature),
            "image/webp" => IsWebp(buffer, bytesRead),
            _ => false
        };
    }

    private static bool IsWebp(byte[] buffer, int bytesRead)
        => StartsWith(buffer, bytesRead, WebpRiffSignature) &&
           bytesRead >= 12 &&
           MatchesAt(buffer, WebpSignature, 8);

    private static bool StartsWith(byte[] buffer, int bytesRead, byte[] signature)
    {
        if (bytesRead < signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (buffer[i] != signature[i])
                return false;
        }

        return true;
    }

    private static bool MatchesAt(byte[] buffer, byte[] signature, int offset)
    {
        if (buffer.Length < offset + signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (buffer[offset + i] != signature[i])
                return false;
        }

        return true;
    }

    private static async Task<int> ReadAtLeastAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken);
            if (read == 0)
                break;

            totalRead += read;
        }

        return totalRead;
    }
}
