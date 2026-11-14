using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Features.Quotes.Shared.Pdf;

/// <summary>What the opening text needs to know about one item the client sees.</summary>
public sealed record QuotePdfIntroItem(bool IsSewing, bool IsPpe, string Name, IReadOnlyList<string> Fabrics);

/// <summary>The opening text case of a КП and the names its marks take.</summary>
public sealed record QuotePdfIntroChoice(
    QuotePdfIntroCase Case,
    string? SewingModel = null,
    string? FabricName = null,
    string? PpeItem = null);

/// <summary>
/// «Вступний текст у КП» (R-35): picks the text by the items the client sees, as the old Calculator did, and fills
/// its marks. Embroidery and other items do not change the case; a КП with neither sewing nor PPE gets the fallback.
/// </summary>
public static class QuotePdfIntro
{
    public static QuotePdfIntroChoice Choose(IReadOnlyList<QuotePdfIntroItem> items)
    {
        var sewing = items.Where(x => x.IsSewing).ToArray();
        var ppe = items.Where(x => x.IsPpe).ToArray();
        var oneSewing = sewing is [{ Fabrics.Count: 1 }] ? sewing[0] : null;

        return (sewing.Length, ppe.Length) switch
        {
            (0, 1) => new(QuotePdfIntroCase.OnePpe, PpeItem: ppe[0].Name),
            (1, 0) when oneSewing is not null =>
                new(QuotePdfIntroCase.OneSewing, oneSewing.Name, FabricName(oneSewing.Fabrics[0])),
            (1, 1) when oneSewing is not null =>
                new(QuotePdfIntroCase.OneSewingOnePpe, oneSewing.Name, FabricName(oneSewing.Fabrics[0]), ppe[0].Name),
            (> 0, 0) => new(QuotePdfIntroCase.Sewing),
            (0, > 1) => new(QuotePdfIntroCase.Ppe),
            (> 0, > 0) => new(QuotePdfIntroCase.SewingAndPpe),
            _ => new(QuotePdfIntroCase.Fallback),
        };
    }

    /// <summary>
    /// Fills {recipient} («для <client>», «для вас» without a client), {sewingModel}, {fabricName}, {ppeItem} and the
    /// older {client}.
    /// </summary>
    public static string Fill(string template, QuotePdfIntroChoice choice, string? client) =>
        template
            .Replace("{recipient}", client is null ? "для вас" : $"для {client}", StringComparison.Ordinal)
            .Replace(" для {client}", client is null ? "" : $" для {client}", StringComparison.Ordinal)
            .Replace("{client}", client ?? "", StringComparison.Ordinal)
            .Replace("{sewingModel}", choice.SewingModel ?? "", StringComparison.Ordinal)
            .Replace("{fabricName}", choice.FabricName ?? "", StringComparison.Ordinal)
            .Replace("{ppeItem}", choice.PpeItem ?? "", StringComparison.Ordinal);

    /// <summary>«у тканині {fabricName}»: a variant titled «Тканина «Грета»» gives «Грета».</summary>
    private static string FabricName(string title)
    {
        const string prefix = "Тканина ";
        var name = title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? title[prefix.Length..] : title;
        return name.Trim();
    }
}
