using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using BuildingBlocks.Application.Caching;
using Content.Application.Features.Catalog.Shared.Snapshot;

namespace Content.Application.Features.Catalog.Shared;

public interface IPublicCatalogSource
{
    Task<PublicCatalogSnapshot> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public API answers from (CNT-002 R-02, R-09). Building it reads every Catalog product, so it is read through
/// <see cref="PublicSiteCache"/>: an anonymous caller cannot make the server rebuild it on every request, and a change made
/// in the Admin reaches the site within that time.
/// </summary>
public sealed class PublicCatalogSource(IContentDbContext db, IContentCatalogReader catalog, IReadCache cache)
    : IPublicCatalogSource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-catalog");

    public async Task<PublicCatalogSnapshot> GetAsync(CancellationToken cancellationToken)
        => await cache.GetOrCreateAsync(Entry, BuildAsync, cancellationToken);

    private async ValueTask<PublicCatalogSnapshot> BuildAsync(CancellationToken cancellationToken)
    {
        var pages = await db.ProductPages.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .ToDictionaryAsync(x => x.ProductId.Value, cancellationToken);
        var products = pages.Count == 0
            ? []
            : (await catalog.GetAllAsync(cancellationToken))
            .Where(x => x.IsActive && pages.ContainsKey(x.Id))
            .Select(x => new PublicCatalogEntry(x, PublicProductPageView.From(pages[x.Id])))
            .ToArray();
        var categories = await CategoriesAsync(products, cancellationToken);
        if (products.Length == 0) return new PublicCatalogSnapshot([], [], categories);
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

        return new PublicCatalogSnapshot(products, sections, categories);
    }

    private async Task<IReadOnlyList<PublicCatalogSnapshotCategory>> CategoriesAsync(
        IReadOnlyList<PublicCatalogEntry> products, CancellationToken cancellationToken)
    {
        var shown = (await db.SiteCategories.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(cancellationToken))
            .Select(PublicSiteCategoryView.From).ToList();
        if (shown.Count == 0) return [];

        var byProduct = products.ToDictionary(x => x.Product.Id);
        var ids = shown.Select(x => x.Id).ToArray();
        var own = (await db.SiteCategoryProducts.AsNoTracking().Where(x => ids.Contains(x.CategoryId))
                .OrderBy(x => x.SortOrder).ThenBy(x => x.ProductId).ToListAsync(cancellationToken))
            .Where(x => byProduct.ContainsKey(x.ProductId.Value))
            .ToLookup(x => x.CategoryId, x => byProduct[x.ProductId.Value]);

        var result = new List<PublicCatalogSnapshotCategory>();
        foreach (var top in shown.Where(x => x.ParentId is null))
        {
            var children = shown.Where(x => x.ParentId == top.Id)
                .Select(x => Category(x, own[x.Id])).ToArray();
            result.Add(Category(top, own[top.Id].Concat(children.SelectMany(x => x.Products)), own[top.Id]));
            result.AddRange(children);
        }

        return result;
    }

    private static PublicCatalogSnapshotCategory Category(PublicSiteCategoryView category,
        IEnumerable<PublicCatalogEntry> products, IEnumerable<PublicCatalogEntry>? own = null)
    {
        var listed = products.DistinctBy(x => x.Product.Id).ToArray();
        return new PublicCatalogSnapshotCategory(category, listed,
            (own ?? listed).Select(x => x.Product.Id).ToHashSet());
    }
}
