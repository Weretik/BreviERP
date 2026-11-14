using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

public sealed class ImportOldSiteRedirectsHandler(
    IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<ImportOldSiteRedirectsCommand, Result<OldSiteRedirectImportResponse>>
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Kept = "Kept";
    public const string Same = "Same";
    public const string Unresolved = "Unresolved";

    public const string BadAddress = "Некоректна адреса.";
    public const string BadTarget = "Нова адреса у файлі не є сторінкою нового сайту (/uk або /ru).";
    public const string LivePage = "Це адреса сторінки нового сайту; переадресація не потрібна.";
    public const string ManualKept = "Переадресацію змінено вручну; її збережено.";

    public async ValueTask<Result<OldSiteRedirectImportResponse>> Handle(ImportOldSiteRedirectsCommand command,
        CancellationToken cancellationToken)
    {
        var resolver = await ResolverAsync(cancellationToken);
        var existing = (await db.SiteRedirects.ToListAsync(cancellationToken))
            .ToDictionary(x => x.FromPath, StringComparer.Ordinal);
        var rows = new List<OldSiteRedirectRow>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var now = clock.GetUtcNow();
        int created = 0, updated = 0, kept = 0, unresolved = 0;

        foreach (var item in command.Items)
        {
            var from = SiteRedirectRules.Normalize(item.From);
            if (from is null)
            {
                unresolved++;
                rows.Add(new OldSiteRedirectRow(item.From, null, BadAddress, Unresolved));
                continue;
            }

            if (!seen.Add(from)) continue;
            string? to;
            string note;
            if (string.IsNullOrWhiteSpace(item.To))
            {
                (to, note) = resolver.Resolve(from);
            }
            else
            {
                to = SiteRedirectRules.Normalize(item.To);
                note = OldSiteRedirectResolver.ByFile;
                if (!SiteRedirectRules.IsSitePath(to)) (to, note) = (null, BadTarget);
            }

            if (to is null)
            {
                unresolved++;
                rows.Add(new OldSiteRedirectRow(from, null, note, Unresolved));
                continue;
            }

            if (to == from)
            {
                rows.Add(new OldSiteRedirectRow(from, to, LivePage, Same));
                continue;
            }

            if (existing.TryGetValue(from, out var current))
            {
                if (current.ToPath == to)
                {
                    rows.Add(new OldSiteRedirectRow(from, to, note, Same));
                }
                else if (!command.Overwrite || current.Source == SiteRedirectSource.Manual)
                {
                    kept++;
                    rows.Add(new OldSiteRedirectRow(from, current.ToPath,
                        current.Source == SiteRedirectSource.Manual ? ManualKept : current.Note, Kept));
                }
                else
                {
                    updated++;
                    rows.Add(new OldSiteRedirectRow(from, to, note, Updated));
                    if (!command.DryRun)
                    {
                        var changed = current.Change(from, to, note, SiteRedirectSource.OldSite, command.UpdatedBy, now);
                        if (changed.IsFailure) return changed.Error!.ToResult<OldSiteRedirectImportResponse>();
                    }
                }

                continue;
            }

            created++;
            rows.Add(new OldSiteRedirectRow(from, to, note, Created));
            if (!command.DryRun)
            {
                var made = SiteRedirect.Create(from, to, SiteRedirectSource.OldSite, note, command.UpdatedBy, now);
                if (made.IsFailure) return made.Error!.ToResult<OldSiteRedirectImportResponse>();
                db.SiteRedirects.Add(made.Value);
            }
        }

        if (existing.Count + created > SiteRedirectRules.MaxRedirects) return Result.Conflict(RedirectMessages.TooMany);
        if (!command.DryRun) await db.SaveChangesAsync(cancellationToken);
        return new OldSiteRedirectImportResponse(command.DryRun, command.Items.Count, created, updated, kept, unresolved, rows);
    }

    private async Task<OldSiteRedirectResolver> ResolverAsync(CancellationToken cancellationToken)
    {
        var articles = (await db.Articles.AsNoTracking().Select(x => x.Slug).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        var categories = await db.SiteCategories.AsNoTracking().Select(x => new { x.Id, x.Slug })
            .ToListAsync(cancellationToken);
        var categorySlug = categories.ToDictionary(x => x.Id, x => x.Slug);
        var pages = await db.ProductPages.AsNoTracking().Select(x => new { x.ProductId, x.Slug })
            .ToDictionaryAsync(x => x.ProductId, x => x.Slug, cancellationToken);
        var firstCategory = (await db.SiteCategoryProducts.AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => categorySlug.GetValueOrDefault(x.OrderBy(link => link.SortOrder).First().CategoryId));

        var products = new Dictionary<string, ResolverProduct>(StringComparer.Ordinal);
        foreach (var product in await catalog.GetAllAsync(cancellationToken))
        {
            var last = OldSiteRedirectResolver.LastPart(product.OldSiteUrl);
            if (last.Length == 0) continue;
            var resolved = new ResolverProduct(product.Name, pages.GetValueOrDefault(product.Id),
                firstCategory.GetValueOrDefault(product.Id));
            // Two cards may share an old link; the one with a page wins.
            if (!products.TryGetValue(last, out var other) || (other.PageSlug is null && resolved.PageSlug is not null))
                products[last] = resolved;
        }

        return new OldSiteRedirectResolver(articles, categories.Select(x => x.Slug).ToHashSet(StringComparer.Ordinal),
            products);
    }
}
