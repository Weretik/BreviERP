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
        if (document.Title is null || document.Title.Length > 500 || document.Paragraphs is null || document.Paragraphs.Count > 500 || document.Paragraphs.Any(x => x is null || x.Length > 8000) || document.Images is null || document.Images.Count > 10 || document.Images.Any(x => x is null) || document.Images.Sum(x => (long)x.Bytes.Length) > PrivateFilePolicy.MaximumBytes)
            throw new ArgumentException("Public document exceeds renderer input limits.", nameof(document));
        var html = new StringBuilder("""<!doctype html><html lang=uk><head><meta charset=utf-8><meta http-equiv=Content-Security-Policy content="default-src 'none'; style-src 'unsafe-inline'; img-src data:; font-src data:; script-src 'none'; connect-src 'none'"><style>""");
        html.Append("@font-face{font-family:DocumentFont;src:url(data:font/ttf;base64,").Append(Convert.ToBase64String(font)).Append(") format('truetype')}@page{size:A4;margin:18mm}body{font-family:DocumentFont;font-size:12pt;line-height:1.45;color:#111}h1{font-size:20pt}p{white-space:pre-wrap;overflow-wrap:anywhere;orphans:3;widows:3}figure{break-inside:avoid}img{max-width:100%;max-height:220mm}figcaption{font-size:10pt}</style><title>");
        html.Append(WebUtility.HtmlEncode(document.Title)).Append("</title></head><body><h1>").Append(WebUtility.HtmlEncode(document.Title)).Append("</h1>");
        foreach (var text in document.Paragraphs) html.Append("<p>").Append(WebUtility.HtmlEncode(text)).Append("</p>");
        foreach (var image in document.Images)
        {
            if (image.AlternativeText is null || image.AlternativeText.Length > 1000 || image.ContentType is not ("image/png" or "image/jpeg") || !PrivateFilePolicy.MatchesAllowedContent(image.Bytes.Span, image.ContentType))
                throw new ArgumentException("Renderer requires validated inline PNG/JPEG images.", nameof(document));
            html.Append("<figure><img src='data:").Append(image.ContentType).Append(";base64,").Append(Convert.ToBase64String(image.Bytes.Span)).Append("' alt='").Append(WebUtility.HtmlEncode(image.AlternativeText)).Append("'><figcaption>").Append(WebUtility.HtmlEncode(image.AlternativeText)).Append("</figcaption></figure>");
        }
        return html.Append("</body></html>").ToString();
    }
}
