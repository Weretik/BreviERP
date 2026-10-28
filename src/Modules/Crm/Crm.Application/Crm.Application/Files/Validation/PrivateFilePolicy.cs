namespace Crm.Application.Files.Validation;

public static class PrivateFilePolicy
{
    public const int MaximumBytes = 20 * 1024 * 1024;
    public const int MaximumFilesPerConfiguration = 30;

    public static bool MatchesAllowedContent(ReadOnlySpan<byte> content, string declaredContentType)
    {
        if (content.Length == 0 || content.Length > MaximumBytes) return false;
        return declaredContentType switch
        {
            "application/pdf" => content.StartsWith("%PDF-"u8),
            "image/jpeg" => content.StartsWith(new byte[] { 255, 216, 255 }),
            "image/png" => content.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            _ => false
        };
    }
}
