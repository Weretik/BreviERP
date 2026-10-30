using Crm.Application.Abstractions.Documents;
using Crm.Application.Files.Validation;

namespace Crm.Infrastructure.Documents.Rendering;

/// <summary>Checks the allowlisted renderer input before any HTML or browser work.</summary>
internal static class PublicDocumentProjectionValidator
{
    internal static void Validate(PublicDocumentProjection document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Title is null || document.Title.Length > 500 || document.Paragraphs is null || document.Paragraphs.Count > 500 || document.Paragraphs.Any(x => x is null || x.Length > 8000) || document.Images is null || document.Images.Count + (document.HeaderLogo is null ? 0 : 1) > 10 || document.Images.Any(x => x is null) || document.Images.Sum(x => (long)x.Bytes.Length) + (document.HeaderLogo?.Bytes.Length ?? 0) > PrivateFilePolicy.MaximumBytes ||
            (document.Pages is not null && (document.Pages.Count is < 1 or > 2 || document.Paragraphs.Count != 0 || document.Images.Count != 0 || document.Pages.Any(x => !ValidPage(x)))))
            throw new ArgumentException("Public document exceeds renderer input limits.", nameof(document));
        if (document.TextGroups is { } groups &&
            (document.Pages is not null || groups.Count is < 1 or > 102 ||
             groups.Any(group => group?.Paragraphs is null || group.Paragraphs.Count == 0) ||
             !groups.SelectMany(group => group.Paragraphs).SequenceEqual(document.Paragraphs)))
            throw new ArgumentException("Public paragraph groups must match the allowlisted projection.", nameof(document));
        foreach (var image in document.Images) ValidateImage(image);
        if (document.HeaderLogo is { } logo) ValidateImage(logo);
    }

    private static void ValidateImage(PublicDocumentImage image)
    {
        if (image.AlternativeText is null || image.AlternativeText.Length > 1000 ||
            image.ContentType is not ("image/png" or "image/jpeg") ||
            !PrivateFilePolicy.MatchesAllowedContent(image.Bytes.Span, image.ContentType))
            throw new ArgumentException("Renderer requires validated inline PNG/JPEG images.", nameof(image));
    }

    private static bool ValidPage(PublicDocumentPage? page) =>
        page is not null && ValidText(page.Heading) &&
        page.IntroParagraphs is { Count: <= 4 } && page.IntroParagraphs.All(ValidText) &&
        page.FooterParagraphs is { Count: <= 6 } && page.FooterParagraphs.All(ValidText) &&
        page.Products is { Count: 4 } && page.Products.All(card => card is not null &&
            ValidText(card.ModelName) && ValidText(card.FabricName) &&
            ValidText(card.FabricComposition) && ValidText(card.FabricDensity) &&
            card.LargeWholesale > 0 && IsBreviProductUrl(card.ProductUrl));

    private static bool ValidText(string? value) => value is not null && value.Length <= 1000;

    private static bool IsBreviProductUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var url) &&
        url.Scheme == Uri.UriSchemeHttps && url.UserInfo.Length == 0 &&
        url.AbsolutePath is not ("" or "/") &&
        (url.Host.Equals("brevi.com.ua", StringComparison.OrdinalIgnoreCase) ||
         url.Host.EndsWith(".brevi.com.ua", StringComparison.OrdinalIgnoreCase));

}
