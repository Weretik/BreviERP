using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Introductions.Building;

/// <summary>Names of the introduction pages as the Catalog and the client see them.</summary>
public static class QuoteIntroPages
{
    /// <summary>The Catalog season of the models of a page.</summary>
    public static string CatalogSeason(QuoteIntroSeason season) => season == QuoteIntroSeason.Summer ? "Літо" : "Зима";

    public static string Title(QuoteIntroSeason season) =>
        season == QuoteIntroSeason.Summer ? "Літній спецодяг" : "Зимовий спецодяг";

    public static bool IsSeason(string? catalogSeason, QuoteIntroSeason season) =>
        string.Equals(catalogSeason?.Trim(), CatalogSeason(season), StringComparison.OrdinalIgnoreCase);

    /// <summary>The items of each page in layout order: <see cref="QuoteLimits.IntroModelsPerPage"/> per page.</summary>
    public static IEnumerable<(QuoteIntroSeason Season, IReadOnlyList<QuoteItem> Items)> Split(
        QuoteIntroduction introduction, IReadOnlyList<QuoteItem> items)
    {
        var ordered = items.OrderBy(x => x.Position).ToArray();
        var pages = introduction.Pages;
        for (var index = 0; index < pages.Count; index++)
            yield return (pages[index],
                ordered.Skip(index * QuoteLimits.IntroModelsPerPage).Take(QuoteLimits.IntroModelsPerPage).ToArray());
    }
}
