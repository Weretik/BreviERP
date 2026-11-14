using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

/// <summary>
/// One page of product cards of the site, in the order of the Admin list (sewing first, by code).
/// <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>; <paramref name="Language"/> is <c>uk</c> or <c>ru</c>.
/// <paramref name="Category"/> is the address of a shown site category: its products and those of its subcategories
/// come back in the order of the category (CNT-003 R-03); an unknown or hidden category is «not found».
/// </summary>
public sealed record GetPublicCatalogProductsQuery(
    int Page, int PageSize, string Language, string? Type = null, int? SectionId = null, string? Category = null)
    : IQuery<Result<PublicCatalogCardPage>>
{
    public const int MaxPageSize = 100;
}
