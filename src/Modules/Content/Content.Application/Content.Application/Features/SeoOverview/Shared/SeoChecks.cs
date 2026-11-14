using Content.Domain.ProductPages;

namespace Content.Application.Features.SeoOverview.Shared;

/// <summary>Search texts of one language of a page as the site shows them.</summary>
/// <param name="SeoTitle">What staff wrote for search; null when empty.</param>
/// <param name="ShownTitle">The title the site shows: the SEO title or the name.</param>
internal sealed record SeoTexts(string? SeoTitle, string? SeoDescription, string ShownTitle);

/// <summary>A page shown on the site with its texts; <see cref="Ru"/> is null when it has no Russian texts.</summary>
internal sealed record SeoPage(SeoArea Area, string Id, string Name, string Path, SeoTexts Uk, SeoTexts? Ru);

/// <summary>The checks of «SEO-огляд» (CNT-008 R-02): only problems, the lengths search results show in full.</summary>
internal static class SeoChecks
{
    public const int GoodTitle = 60;
    public const int GoodDescription = 160;

    public static IReadOnlyList<SeoIssue> Issues(IReadOnlyList<SeoPage> pages)
    {
        var duplicates = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            [ProductPageRules.Ukrainian] = Repeated(pages.Select(x => x.Uk.ShownTitle)),
            [ProductPageRules.Russian] = Repeated(pages.Where(x => x.Ru is not null).Select(x => x.Ru!.ShownTitle)),
        };

        var issues = new List<SeoIssue>();
        foreach (var page in pages)
        {
            var problems = new List<SeoProblem>();
            Check(page.Uk, ProductPageRules.Ukrainian, duplicates, problems);
            if (page.Ru is null) problems.Add(new SeoProblem(SeoProblemKind.NoRussian, null));
            else Check(page.Ru, ProductPageRules.Russian, duplicates, problems);
            if (problems.Count > 0) issues.Add(new SeoIssue(page.Area, page.Id, page.Name, page.Path, problems));
        }

        return issues;
    }

    private static void Check(SeoTexts texts, string language, Dictionary<string, HashSet<string>> duplicates,
        List<SeoProblem> problems)
    {
        if (texts.SeoTitle is null) problems.Add(new SeoProblem(SeoProblemKind.NoTitle, language));
        else if (texts.SeoTitle.Length > GoodTitle) problems.Add(new SeoProblem(SeoProblemKind.TitleTooLong, language));
        if (texts.SeoDescription is null) problems.Add(new SeoProblem(SeoProblemKind.NoDescription, language));
        else if (texts.SeoDescription.Length > GoodDescription)
            problems.Add(new SeoProblem(SeoProblemKind.DescriptionTooLong, language));
        if (duplicates[language].Contains(Key(texts.ShownTitle)))
            problems.Add(new SeoProblem(SeoProblemKind.DuplicateTitle, language));
    }

    private static HashSet<string> Repeated(IEnumerable<string> titles) =>
        titles.GroupBy(Key).Where(x => x.Count() > 1).Select(x => x.Key).ToHashSet(StringComparer.Ordinal);

    private static string Key(string title) => title.Trim().ToLowerInvariant();
}
