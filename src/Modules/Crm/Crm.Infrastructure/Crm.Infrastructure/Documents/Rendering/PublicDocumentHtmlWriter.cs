using System.Net;
using System.Text;
using Crm.Application.Abstractions.Documents;

namespace Crm.Infrastructure.Documents.Rendering;

internal static class PublicDocumentHtmlWriter
{
    internal static string Write(PublicDocumentProjection document, byte[] font)
    {
        PublicDocumentProjectionValidator.Validate(document);
        var html = new StringBuilder("""<!doctype html><html lang=uk><head><meta charset=utf-8><meta http-equiv=Content-Security-Policy content="default-src 'none'; style-src 'unsafe-inline'; img-src data:; font-src data:; script-src 'none'; connect-src 'none'"><style>""");
        PublicDocumentStyles.Append(html, font, document.Pages is not null);
        html.Append("</style><title>");
        html.Append(WebUtility.HtmlEncode(document.Title)).Append("</title></head><body>");
        if (document.HeaderLogo is { } logo) AppendImage(html, logo, true);
        html.Append("<h1>").Append(WebUtility.HtmlEncode(document.Title)).Append("</h1>");
        if (document.Pages is { } pages) IntroductionDocumentHtmlWriter.Append(html, pages);
        else
        {
            if (document.TextGroups is { } textGroups)
            {
                foreach (var group in textGroups)
                {
                    html.Append("<section class='quote-group'>");
                    foreach (var text in group.Paragraphs)
                        html.Append("<p>").Append(WebUtility.HtmlEncode(text)).Append("</p>");
                    html.Append("</section>");
                }
            }
            else foreach (var text in document.Paragraphs)
                html.Append("<p>").Append(WebUtility.HtmlEncode(text)).Append("</p>");
            foreach (var image in document.Images) AppendImage(html, image, false);
        }
        return html.Append("</body></html>").ToString();
    }

    private static void AppendImage(StringBuilder html, PublicDocumentImage image, bool isLogo)
    {
        html.Append(isLogo ? "<header><img class='brand-logo' src='data:" : "<figure><img src='data:")
            .Append(image.ContentType).Append(";base64,")
            .Append(Convert.ToBase64String(image.Bytes.Span)).Append("' alt='")
            .Append(WebUtility.HtmlEncode(image.AlternativeText)).Append("'>");
        if (isLogo) html.Append("</header>");
        else html.Append("<figcaption>").Append(WebUtility.HtmlEncode(image.AlternativeText))
            .Append("</figcaption></figure>");
    }
}
