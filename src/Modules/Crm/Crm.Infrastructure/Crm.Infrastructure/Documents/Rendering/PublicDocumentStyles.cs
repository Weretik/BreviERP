using System.Text;

namespace Crm.Infrastructure.Documents.Rendering;

/// <summary>Shared print CSS and the compact seasonal overrides validated by PDF acceptance.</summary>
internal static class PublicDocumentStyles
{
    internal static void Append(StringBuilder html, byte[] font, bool isIntroduction)
    {
        html.Append("@font-face{font-family:DocumentFont;src:url(data:font/ttf;base64,").Append(Convert.ToBase64String(font)).Append(") format('truetype')}")
            .Append("@page{size:A4;margin:18mm}")
            .Append("body{font-family:DocumentFont;font-size:12pt;line-height:1.45;color:#111}")
            .Append("h1{font-size:20pt;overflow-wrap:anywhere}")
            .Append("p{white-space:pre-wrap;overflow-wrap:anywhere;orphans:3;widows:3}")
            .Append(".quote-group{break-inside:avoid}")
            .Append("figure{break-inside:avoid}")
            .Append("img{max-width:100%;max-height:220mm}")
            .Append(".brand-logo{max-width:80mm;max-height:38mm}")
            .Append("figcaption{font-size:10pt}")
            .Append(".intro-page{break-inside:avoid}")
            .Append(".intro-page+.intro-page{break-before:page}")
            .Append(".intro-page h2{font-size:16pt;margin:0 0 5mm}")
            .Append(".intro-page .intro-lead{font-size:10pt;margin:0 0 2mm}")
            .Append(".intro-grid{display:grid;grid-template-columns:1fr 1fr;gap:4mm;margin:4mm 0}")
            .Append(".intro-card{border:1px solid #bbb;padding:4mm;min-height:72mm;break-inside:avoid;font-size:10pt;line-height:1.3}")
            .Append(".intro-card h3{overflow-wrap:anywhere;font-size:13pt;margin:0 0 3mm}")
            .Append(".intro-card p{margin:1mm 0}")
            .Append(".intro-card a{color:#134c78;overflow-wrap:anywhere}")
            .Append(".intro-price{font-weight:bold;font-size:12pt}")
            .Append(".intro-footer{font-size:9pt;line-height:1.2}")
            .Append(".intro-footer p{margin:1mm 0}");
        // The seasonal layout reserves room for four cards on its first page too.
        if (isIntroduction)
            html.Append(".brand-logo{max-width:60mm;max-height:20mm}h1{font-size:16pt;margin:0 0 3mm}header{margin:0 0 2mm}.intro-card{min-height:65mm;padding:3mm}");
    }
}
