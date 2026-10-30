using System.Net;
using System.Text;
using Crm.Application.Abstractions.Documents;

namespace Crm.Infrastructure.Documents.Rendering;

/// <summary>Escaped seasonal four-card markup; sizing rules live in PublicDocumentStyles.</summary>
internal static class IntroductionDocumentHtmlWriter
{
    internal static void Append(StringBuilder html, IReadOnlyList<PublicDocumentPage> pages)
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

}
