using Crm.Application.Abstractions.Documents;
using Crm.Application.Files.Validation;

namespace Crm.Infrastructure.Documents.Assets;

// Only the provisioned local Brevi wordmark can enter a customer document.
public sealed class ConfiguredQuoteBrandAssetResolver(string? wordmarkPath) : IQuoteBrandAssetResolver
{
    private const int MaximumLogoBytes = 2 * 1024 * 1024;

    public async Task<PublicDocumentImage?> ResolveAsync(string assetKey, CancellationToken cancellationToken)
    {
        if (assetKey != "brevi-wordmark" || string.IsNullOrWhiteSpace(wordmarkPath) ||
            !Path.IsPathFullyQualified(wordmarkPath)) return null;
        var file = new FileInfo(wordmarkPath);
        if (!file.Exists || file.Length is <= 0 or > MaximumLogoBytes ||
            (file.Attributes & FileAttributes.ReparsePoint) != 0) return null;
        var contentType = file.Extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => null
        };
        if (contentType is null) return null;
        try
        {
            var bytes = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            if (!PrivateFilePolicy.MatchesAllowedContent(bytes, contentType)) return null;
            return new PublicDocumentImage("Логотип Brevi", contentType, bytes);
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }
}
