using Crm.Application.Abstractions.Documents;

namespace Crm.Application.Documents.Projection;

/// <summary>Conservative wrapping budgets for the fixed four-card A4 seasonal layout.
/// Ordinary multipage quotes retain their own larger text limits. Text is never truncated.</summary>
public static class IntroductionDocumentLayoutPolicy
{
    public static void Validate(PublicDocumentProjection document)
    {
        if (Lines(document.Title, 65) > 2 || document.Pages is null ||
            document.Pages.Any(page =>
                page.IntroParagraphs.Sum(text => Lines(text, 75)) > 4 ||
                page.FooterParagraphs.Sum(text => Lines(text, 90)) > 6 ||
                page.Products.Any(card =>
                    Lines(card.ModelName, 30) * 1.3 +
                    Lines("Тканина: " + card.FabricName, 42) +
                    Lines("Склад: " + card.FabricComposition, 42) +
                    Lines("Щільність: " + card.FabricDensity, 42) + 2.5 > 14)))
            throw new InvalidOperationException("introduction_public_layout_too_large");
    }

    private static int Lines(string text, int charactersPerLine) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n')
            .Split('\n').Sum(line => Math.Max(1, (line.Length + charactersPerLine - 1) / charactersPerLine));
}
