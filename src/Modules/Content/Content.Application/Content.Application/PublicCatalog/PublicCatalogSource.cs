using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.PublicCatalog;

/// <summary>A product that the site shows: the Catalog product is active and its page is marked «Показувати на сайті».</summary>
public sealed record PublicCatalogEntry(ContentCatalogProduct Product, ProductPage Page);

/// <param name="Sections">Active sections with at least one product on the site, in the order of the reference.</param>
public sealed record PublicCatalogSnapshot(
    IReadOnlyList<PublicCatalogEntry> Products,
    IReadOnlyList<PublicCatalogSnapshotSection> Sections);

public sealed record PublicCatalogSnapshotSection(int Id, string Type, string Name, string Slug, int ProductCount);

public interface IPublicCatalogSource
{
    Task<PublicCatalogSnapshot> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public API answers from (CNT-002 R-02, R-09). Building it reads every Catalog product, so it is kept for
/// <see cref="Lifetime"/>: an anonymous caller cannot make the server rebuild it on every request, and a change made
/// in the Admin reaches the site within that time.
/// </summary>
public sealed class PublicCatalogSource(IContentDbContext db, IContentCatalogReader catalog, IMemoryCache cache)
    : IPublicCatalogSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-catalog";

    public async Task<PublicCatalogSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicCatalogSnapshot? cached) && cached is not null) return cached;
        var snapshot = await BuildAsync(cancellationToken);
        cache.Set(Key, snapshot, Lifetime);
        return snapshot;
    }

    private async Task<PublicCatalogSnapshot> BuildAsync(CancellationToken cancellationToken)
    {
        var pages = await db.ProductPages.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        if (pages.Count == 0) return new PublicCatalogSnapshot([], []);

        var products = (await catalog.GetAllAsync(cancellationToken))
            .Where(x => x.IsActive && pages.ContainsKey(x.Id))
            .Select(x => new PublicCatalogEntry(x, pages[x.Id]))
            .ToArray();
        var counts = products.Where(x => x.Product.SectionId is not null)
            .GroupBy(x => x.Product.SectionId!.Value).ToDictionary(x => x.Key, x => x.Count());

        var sections = new List<PublicCatalogSnapshotSection>();
        var slugs = new HashSet<string>(StringComparer.Ordinal);
        foreach (var section in await catalog.GetSectionsAsync(cancellationToken))
        {
            if (!section.IsActive || !counts.TryGetValue(section.Id, out var count)) continue;
            var slug = ProductPageRules.SlugFrom(section.Name);
            // Two sections may share a name (one per type); the id keeps the address unique.
            if (!ProductPageRules.IsValidSlug(slug) || !slugs.Add(slug))
            {
                slug = ProductPageRules.SlugFrom($"{section.Name} {section.Id}");
                if (!ProductPageRules.IsValidSlug(slug)) slug = $"section-{section.Id}";
                slugs.Add(slug);
            }

            sections.Add(new PublicCatalogSnapshotSection(section.Id, section.Type, section.Name, slug, count));
        }

        return new PublicCatalogSnapshot(products, sections);
    }
}
