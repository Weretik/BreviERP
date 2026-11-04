namespace Catalog.Application.Features.Product.Overview.GetList.DTOs;

/// <summary>
/// One page of a «Товари» tab. <paramref name="Total"/> counts the rows that match the search and filters,
/// <paramref name="All"/> every row of the type. <paramref name="Sections"/> and <paramref name="WithoutPrice"/> count
/// every row of the type, for the filter chips.
/// </summary>
public sealed record ProductOverviewPage(
    IReadOnlyList<ProductOverviewItem> Items,
    int Page,
    int PageSize,
    int Total,
    int All,
    IReadOnlyList<ProductSectionCount> Sections,
    int WithoutPrice);

/// <param name="SectionId">Null counts rows without a section.</param>
public sealed record ProductSectionCount(int? SectionId, int Count);
