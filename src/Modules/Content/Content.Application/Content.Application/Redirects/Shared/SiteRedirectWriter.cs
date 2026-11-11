using Content.Application.Abstractions;
using Content.Domain.Redirects;

namespace Content.Application.Redirects.Shared;

/// <summary>
/// Keeps old addresses working when staff change the address of a page (CNT-005 R-05): the old address of each
/// language gets a 301 to the new one, redirects that pointed at the old address point at the new one, and a
/// redirect from the new address is removed because that page is live again. Nothing is saved here.
/// </summary>
public static class SiteRedirectWriter
{
    /// <summary>Product pages: <c>/uk/product/&lt;slug&gt;</c>.</summary>
    public const string Products = "/product";

    /// <summary>Site categories: <c>/uk/catalog/&lt;slug&gt;</c>.</summary>
    public const string Categories = "/catalog";

    /// <summary>Articles: <c>/uk/articles/&lt;slug&gt;</c>.</summary>
    public const string Articles = "/articles";

    public static async Task SlugChangedAsync(IContentDbContext db, string section, string oldSlug, string newSlug,
        string updatedBy, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (oldSlug == newSlug) return;
        foreach (var locale in SiteRedirectRules.SiteLocales)
        {
            var from = $"{locale}{section}/{oldSlug}";
            var to = $"{locale}{section}/{newSlug}";
            var touched = await db.SiteRedirects
                .Where(x => x.FromPath == from || x.FromPath == to || x.ToPath == from).ToListAsync(cancellationToken);
            db.SiteRedirects.RemoveRange(touched.Where(x => x.FromPath == to));
            foreach (var pointing in touched.Where(x => x.ToPath == from && x.FromPath != to))
                pointing.Change(pointing.FromPath, to, pointing.Note, pointing.Source, updatedBy, now);

            var own = touched.FirstOrDefault(x => x.FromPath == from);
            if (own is null)
                db.SiteRedirects.Add(new SiteRedirect(from, to, SiteRedirectSource.SlugChange, "Нова адреса сторінки",
                    updatedBy, now));
            else
                own.Change(from, to, "Нова адреса сторінки", SiteRedirectSource.SlugChange, updatedBy, now);
        }
    }
}
