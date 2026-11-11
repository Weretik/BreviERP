using Content.Domain.ProductPages;

namespace Content.Domain.SitePages;

/// <summary>One frequent question of a fixed site page with its answer, in one language (CNT-010).</summary>
public sealed record SiteFaqItem(string Question, string Answer)
{
    /// <summary>Trims both texts and drops a question without its answer or an answer without its question.</summary>
    public static IReadOnlyList<SiteFaqItem> Clean(IReadOnlyList<SiteFaqItem>? items) =>
        (items ?? [])
            .Where(x => x is not null)
            .Select(x => (Question: SitePage.Clean(x.Question), Answer: CleanAnswer(x.Answer)))
            .Where(x => x.Question is not null && x.Answer is not null)
            .Select(x => new SiteFaqItem(x.Question!, x.Answer!))
            .ToArray();

    /// <summary>An answer is one paragraph: lines are joined like in a title.</summary>
    private static string? CleanAnswer(string? value) => SitePage.Clean(value);
}

/// <summary>What staff type for one language of a fixed page.</summary>
public sealed record SitePageTextInput(
    string? Title,
    string? Description,
    IReadOnlyList<ProductTextBlock>? Text,
    IReadOnlyList<SiteFaqItem>? Faq);
