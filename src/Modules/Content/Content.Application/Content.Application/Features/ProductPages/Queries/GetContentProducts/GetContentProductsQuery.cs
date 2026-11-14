using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Queries.GetContentProducts;

/// <summary>
/// One page of «Контент → Товари»: every Catalog product with the state of its site page. <paramref name="Search"/>
/// matches the code, the name or the slug; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>;
/// <paramref name="OnSite"/> keeps products that are (true) or are not (false) on the site; <paramref name="CategoryId"/>
/// keeps products of that site category or of its subcategories, and <see cref="WithoutCategory"/> those in no category.
/// </summary>
public sealed record GetContentProductsQuery(
    int Page, int PageSize, string? Search = null, string? Type = null, int? SectionId = null, bool? OnSite = null,
    int? CategoryId = null)
    : IQuery<Result<ContentProductPage>>
{
    /// <summary>The <see cref="CategoryId"/> that keeps products in no site category.</summary>
    public const int WithoutCategory = 0;
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 200;
    public static readonly string[] Types = ["Sewing", "Ppe"];
}
