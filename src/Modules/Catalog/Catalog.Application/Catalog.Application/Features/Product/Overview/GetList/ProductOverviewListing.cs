using Catalog.Application.Features.Product.Overview.GetList.DTOs;

namespace Catalog.Application.Features.Product.Overview.GetList;

/// <summary>Search, filters, sort and page of the «Товари» rows (CAT-013).</summary>
public static class ProductOverviewListing
{
    public static IEnumerable<ProductOverviewItem> Filter(
        IEnumerable<ProductOverviewItem> items, GetProductOverviewPageQuery query)
    {
        var search = query.Search?.Trim();
        return items.Where(x =>
            (query.SectionId is { } section ? x.SectionId == section : !query.WithoutSection || x.SectionId is null)
            && (!query.WithoutPrice || x.Prices is null)
            && (string.IsNullOrEmpty(search) || Matches(x, search)));
    }

    public static IEnumerable<ProductOverviewItem> Sort(
        IEnumerable<ProductOverviewItem> items, ProductOverviewSort sort, bool descending)
    {
        var ordered = sort switch
        {
            ProductOverviewSort.Name => Direction(items, x => x.Name, StringComparer.CurrentCulture, descending),
            // Rows without a price stay last in both directions.
            ProductOverviewSort.Price => Direction(items.OrderBy(x => x.Prices is null), x => x.Prices?.Retail,
                Comparer<decimal?>.Default, descending),
            ProductOverviewSort.Updated => Direction(items, x => x.UpdatedAtUtc, Comparer<DateTimeOffset>.Default,
                descending),
            _ => Direction(items.OrderBy(x => x.Code is null), x => x.Code, ProductCodeComparer.Instance, descending)
        };
        return ordered.ThenBy(x => x.Name, StringComparer.CurrentCulture).ThenBy(x => x.Id);
    }

    public static IReadOnlyList<ProductSectionCount> CountSections(IEnumerable<ProductOverviewItem> items)
        => items.GroupBy(x => x.SectionId).Select(x => new ProductSectionCount(x.Key, x.Count()))
            .OrderBy(x => x.SectionId is null).ThenBy(x => x.SectionId).ToList();

    private static IOrderedEnumerable<ProductOverviewItem> Direction<TKey>(
        IEnumerable<ProductOverviewItem> items, Func<ProductOverviewItem, TKey> key, IComparer<TKey> comparer,
        bool descending)
    {
        if (items is IOrderedEnumerable<ProductOverviewItem> ordered)
            return descending ? ordered.ThenByDescending(key, comparer) : ordered.ThenBy(key, comparer);
        return descending ? items.OrderByDescending(key, comparer) : items.OrderBy(key, comparer);
    }

    private static bool Matches(ProductOverviewItem item, string search)
        => Contains(item.Code, search) || Contains(item.Name, search) || Contains(item.QuoteDescription, search)
            || item.Variants.Any(v => Contains(v.SaleSku, search) || Contains(v.FabricName, search));

    private static bool Contains(string? value, string search)
        => value is not null && value.Contains(search, StringComparison.CurrentCultureIgnoreCase);
}
