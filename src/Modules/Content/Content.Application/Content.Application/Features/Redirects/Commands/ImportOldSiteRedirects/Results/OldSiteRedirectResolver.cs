using Content.Domain.Redirects;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects.Results;

/// <summary>A product of «Контент → Товари» as the resolver needs it: its page address and its first site category.</summary>
internal sealed record ResolverProduct(string Name, string? PageSlug, string? CategorySlug);

/// <summary>
/// Finds the page of the new site for an address of the old site (CNT-005 R-04). An address under <c>/ru/</c> is
/// resolved without that prefix and sent to the Russian page. The last part of the address is tried as an article,
/// a site category and a product (its old-site link); then the deepest part that is a site category. A product
/// without a page goes to its first site category.
/// </summary>
internal sealed class OldSiteRedirectResolver(
    IReadOnlySet<string> articleSlugs,
    IReadOnlySet<string> categorySlugs,
    IReadOnlyDictionary<string, ResolverProduct> productsByLastPart)
{
    public const string ByFile = "Вказано у файлі";
    public const string Article = "Стаття";
    public const string Category = "Розділ";
    public const string NearestCategory = "Найближчий розділ";
    public const string NotFound = "Сторінку на новому сайті не знайдено. Вкажіть нову адресу вручну.";

    public (string? To, string Note) Resolve(string from)
    {
        var locale = "uk";
        var rest = from;
        if (from == "/ru" || from.StartsWith("/ru/", StringComparison.Ordinal))
        {
            locale = "ru";
            rest = from[3..];
        }

        var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ($"/{locale}", "Головна");
        var last = parts[^1];
        if (articleSlugs.Contains(last)) return ($"/{locale}{SiteRedirectWriter.Articles}/{last}", Article);
        if (categorySlugs.Contains(last)) return ($"/{locale}{SiteRedirectWriter.Categories}/{last}", Category);
        if (productsByLastPart.TryGetValue(last, out var product))
        {
            if (product.PageSlug is not null)
                return ($"/{locale}{SiteRedirectWriter.Products}/{product.PageSlug}", Limit($"Товар «{product.Name}»"));
            if (product.CategorySlug is not null)
                return ($"/{locale}{SiteRedirectWriter.Categories}/{product.CategorySlug}",
                    Limit($"Розділ товару «{product.Name}»"));
        }

        for (var i = parts.Length - 2; i >= 0; i--)
        {
            if (categorySlugs.Contains(parts[i]))
                return ($"/{locale}{SiteRedirectWriter.Categories}/{parts[i]}", NearestCategory);
        }

        return (null, NotFound);
    }

    /// <summary>The last part of an old-site product link, as <see cref="Resolve"/> compares it.</summary>
    public static string LastPart(string? oldSiteUrl) => OldSiteLinks.LastPart(oldSiteUrl);

    private static string Limit(string note) =>
        note.Length <= SiteRedirectRules.NoteMaxLength ? note : note[..(SiteRedirectRules.NoteMaxLength - 1)] + "…";
}
