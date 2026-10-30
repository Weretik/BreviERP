using System.Net;
using System.Text;
using Crm.Application.Abstractions.Documents;
using Crm.Application.Files.Validation;

namespace Crm.Infrastructure.Documents.Rendering;

internal static class PublicDocumentHtmlWriter
{
    internal static string Write(PublicDocumentProjection document, byte[] font)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Title is null || document.Title.Length > 500 || document.Paragraphs is null || document.Paragraphs.Count > 500 || document.Paragraphs.Any(x => x is null || x.Length > 8000) || document.Images is null || document.Images.Count + (document.HeaderLogo is null ? 0 : 1) > 10 || document.Images.Any(x => x is null) || document.Images.Sum(x => (long)x.Bytes.Length) + (document.HeaderLogo?.Bytes.Length ?? 0) > PrivateFilePolicy.MaximumBytes ||
            (document.Pages is not null && (document.Pages.Count is < 1 or > 2 || document.Paragraphs.Count != 0 || document.Images.Count != 0 || document.Pages.Any(x => !ValidPage(x)))))
            throw new ArgumentException("Public document exceeds renderer input limits.", nameof(document));
        var html = new StringBuilder("""<!doctype html><html lang=uk><head><meta charset=utf-8><meta http-equiv=Content-Security-Policy content="default-src 'none'; style-src 'unsafe-inline'; img-src data:; font-src data:; script-src 'none'; connect-src 'none'"><style>""");
        html.Append("@font-face{font-family:DocumentFont;src:url(data:font/ttf;base64,").Append(Convert.ToBase64String(font)).Append(") format('truetype')}@page{size:A4;margin:18mm}body{font-family:DocumentFont;font-size:12pt;line-height:1.45;color:#111}h1{font-size:20pt}p{white-space:pre-wrap;overflow-wrap:anywhere;orphans:3;widows:3}figure{break-inside:avoid}img{max-width:100%;max-height:220mm}.brand-logo{max-width:80mm;max-height:38mm}figcaption{font-size:10pt}.intro-page{break-inside:avoid}.intro-page+.intro-page{break-before:page}.intro-page h2{font-size:16pt;margin:0 0 5mm}.intro-page .intro-lead{font-size:10pt;margin:0 0 2mm}.intro-grid{display:grid;grid-template-columns:1fr 1fr;gap:4mm;margin:4mm 0}.intro-card{border:1px solid #bbb;padding:4mm;min-height:72mm;break-inside:avoid;font-size:10pt;line-height:1.3}.intro-card h3{font-size:13pt;margin:0 0 3mm}.intro-card p{margin:1mm 0}.intro-card a{color:#134c78;overflow-wrap:anywhere}.intro-price{font-weight:bold;font-size:12pt}.intro-footer{font-size:9pt;line-height:1.2}.intro-footer p{margin:1mm 0}</style><title>");
        html.Append(WebUtility.HtmlEncode(document.Title)).Append("</title></head><body>");
        if (document.HeaderLogo is { } logo) AppendImage(html, logo, true);
        html.Append("<h1>").Append(WebUtility.HtmlEncode(document.Title)).Append("</h1>");
        if (document.Pages is { } pages) AppendPages(html, pages);
        else
        {
            foreach (var text in document.Paragraphs) html.Append("<p>").Append(WebUtility.HtmlEncode(text)).Append("</p>");
            foreach (var image in document.Images) AppendImage(html, image, false);
        }
        return html.Append("</body></html>").ToString();
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

    private static void AppendPages(StringBuilder html, IReadOnlyList<PublicDocumentPage> pages)
    {
        foreach (var page in pages)
        {
            html.Append("<section class='intro-page'><h2>").Append(WebUtility.HtmlEncode(page.Heading)).Append("</h2>");
            foreach (var line in page.IntroParagraphs)
                html.Append("<p class='intro-lead'>").Append(WebUtility.HtmlEncode(line)).Append("</p>");
            html.Append("<div class='intro-grid'>");
            foreach (var card in page.Products)
            {
                html.Append("<article class='intro-card'><h3>").Append(WebUtility.HtmlEncode(card.ModelName)).Append("</h3>")
                    .Append("<p>Тканина: ").Append(WebUtility.HtmlEncode(card.FabricName)).Append("</p>")
                    .Append("<p>Склад: ").Append(WebUtility.HtmlEncode(card.FabricComposition)).Append("</p>")
                    .Append("<p>Щільність: ").Append(WebUtility.HtmlEncode(card.FabricDensity)).Append("</p>")
                    .Append("<p class='intro-price'>Великий гурт: ")
                    .Append(card.LargeWholesale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(" грн</p><p><a href='").Append(WebUtility.HtmlEncode(card.ProductUrl))
                    .Append("'>Переглянути модель</a></p></article>");
            }
            html.Append("</div><footer class='intro-footer'>");
            foreach (var line in page.FooterParagraphs)
                html.Append("<p>").Append(WebUtility.HtmlEncode(line)).Append("</p>");
            html.Append("</footer></section>");
        }
    }

    private static void AppendImage(StringBuilder html, PublicDocumentImage image, bool isLogo)
    {
        if (image.AlternativeText is null || image.AlternativeText.Length > 1000 ||
            image.ContentType is not ("image/png" or "image/jpeg") ||
            !PrivateFilePolicy.MatchesAllowedContent(image.Bytes.Span, image.ContentType))
            throw new ArgumentException("Renderer requires validated inline PNG/JPEG images.", nameof(image));
        html.Append(isLogo ? "<header><img class='brand-logo' src='data:" : "<figure><img src='data:")
            .Append(image.ContentType).Append(";base64,")
            .Append(Convert.ToBase64String(image.Bytes.Span)).Append("' alt='")
            .Append(WebUtility.HtmlEncode(image.AlternativeText)).Append("'>");
        if (isLogo) html.Append("</header>");
        else html.Append("<figcaption>").Append(WebUtility.HtmlEncode(image.AlternativeText))
            .Append("</figcaption></figure>");
    }
}
