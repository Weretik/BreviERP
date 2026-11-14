using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.ProductPages.Queries.GetContentProducts;

public sealed class GetContentProductsHandler(IContentDbContext db, IContentCatalogReader catalog)
    : IQueryHandler<GetContentProductsQuery, Result<ContentProductPage>>
{
    public async ValueTask<Result<ContentProductPage>> Handle(GetContentProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await catalog.GetAllAsync(cancellationToken);
        var pages = await db.ProductPages.AsNoTracking().Include(x => x.Texts)
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var categories = (await db.SiteCategoryProducts.AsNoTracking()
                .Select(x => new { x.ProductId, x.CategoryId }).ToListAsync(cancellationToken))
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<int>)x.Select(y => y.CategoryId).Order().ToArray());
        var wanted = await WantedCategoriesAsync(query.CategoryId, cancellationToken);
        var search = query.Search?.Trim();

        var rows = products
            .Where(x => query.Type is null || x.Type == query.Type)
            .Where(x => query.SectionId is null || x.SectionId == query.SectionId)
            .Where(x => InCategory(categories.GetValueOrDefault(x.Id) ?? [], query.CategoryId, wanted))
            .Select(x => ContentProductMapping.Row(x, pages.GetValueOrDefault(x.Id), categories.GetValueOrDefault(x.Id) ?? []))
            .Where(x => query.OnSite is null || x.OnSite == query.OnSite)
            .Where(x => string.IsNullOrEmpty(search) || Matches(x, search))
            .ToArray();

        var items = rows.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArray();
        return new ContentProductPage(items, query.Page, query.PageSize, rows.Length);
    }

    /// <summary>The asked category with every category below it; null when the list is not narrowed to one.</summary>
    private async Task<HashSet<int>?> WantedCategoriesAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is null or GetContentProductsQuery.WithoutCategory) return null;
        var parents = await db.SiteCategories.AsNoTracking().Select(x => new { x.Id, x.ParentId })
            .ToListAsync(cancellationToken);
        var wanted = new HashSet<int> { categoryId.Value };
        for (var added = true; added;)
            added = parents.Where(x => x.ParentId is { } parent && wanted.Contains(parent))
                .Aggregate(false, (any, x) => wanted.Add(x.Id) || any);
        return wanted;
    }

    private static bool InCategory(IReadOnlyList<int> categoryIds, int? categoryId, HashSet<int>? wanted) =>
        categoryId switch
        {
            null => true,
            GetContentProductsQuery.WithoutCategory => categoryIds.Count == 0,
            _ => categoryIds.Any(wanted!.Contains),
        };

    private static bool Matches(ContentProductRow row, string search) =>
        Has(row.Code, search) || Has(row.Name, search) || Has(row.Slug, search);

    private static bool Has(string? value, string search) =>
        value is not null && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
