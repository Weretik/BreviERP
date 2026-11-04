using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Overview.GetList;

/// <summary>
/// One page of a «Товари» tab (CAT-013): the server searches, filters, sorts and pages the rows and returns them with
/// their published prices and fabric variants, so the list needs a single request.
/// </summary>
/// <param name="Search">Code, name, quote description, variant SKU or fabric name; case-insensitive.</param>
/// <param name="SectionId">Only this «Типи і розділи» section.</param>
/// <param name="WithoutSection">Only rows without a section; ignored when <paramref name="SectionId"/> is set.</param>
/// <param name="WithoutPrice">Only rows without a price.</param>
/// <param name="Page">1-based.</param>
public sealed record GetProductOverviewPageQuery(
    ProductType Type,
    string? Search,
    int? SectionId,
    bool WithoutSection,
    bool WithoutPrice,
    ProductOverviewSort Sort,
    bool Descending,
    int Page,
    int PageSize) : IQuery<Result<ProductOverviewPage>>
{
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 200;
}

/// <summary>Code sorts MOD-2 before MOD-10 and article 9 before 17; price sorts by the 1–9 pcs price.</summary>
public enum ProductOverviewSort
{
    Code,
    Name,
    Price,
    Updated
}
