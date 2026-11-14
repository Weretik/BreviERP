using Content.Application.Abstractions;
using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Shared;

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

    public static async Task<DomainResult> SlugChangedAsync(IContentDbContext db, string section, string oldSlug, string newSlug,
        string updatedBy, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (oldSlug == newSlug) return DomainResult.Success();
        foreach (var locale in SiteRedirectRules.SiteLocales)
        {
            var from = $"{locale}{section}/{oldSlug}";
            var to = $"{locale}{section}/{newSlug}";
            var touched = await db.SiteRedirects
                .Where(x => x.FromPath == from || x.FromPath == to || x.ToPath == from).ToListAsync(cancellationToken);
            db.SiteRedirects.RemoveRange(touched.Where(x => x.FromPath == to));
            foreach (var pointing in touched.Where(x => x.ToPath == from && x.FromPath != to))
            {
                var repointed = pointing.Change(pointing.FromPath, to, pointing.Note, pointing.Source, updatedBy, now);
                if (repointed.IsFailure) return repointed;
            }

            var own = touched.FirstOrDefault(x => x.FromPath == from);
            if (own is null)
            {
                var created = SiteRedirect.Create(from, to, SiteRedirectSource.SlugChange, NewAddressNote, updatedBy, now);
                if (created.IsFailure) return created.WithoutValue();
                db.SiteRedirects.Add(created.Value);
            }
            else
            {
                var changed = own.Change(from, to, NewAddressNote, SiteRedirectSource.SlugChange, updatedBy, now);
                if (changed.IsFailure) return changed;
            }
        }

        return DomainResult.Success();
    }

    private const string NewAddressNote = "Нова адреса сторінки";
}
