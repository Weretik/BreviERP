using BuildingBlocks.Api.Result;
using Content.Domain.ProductPages;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;

namespace Content.Api.PublicCatalog;

/// <summary>
/// The catalog of the public site (CNT-002 stage 4, CNT-003 stage 4): read-only, without sign-in. It returns only products marked
/// «Показувати на сайті» and nothing internal. Answers may be kept by the caller for five minutes.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/catalog")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicCatalogController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;
    public const int DefaultPageSize = 24;

    [HttpGet("tree", Name = "getPublicCatalogTree")]
    public async Task<ActionResult<PublicCatalogTree>> Tree(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogTreeQuery(), cancellationToken));

    [HttpGet("products", Name = "getPublicCatalogProducts")]
    public async Task<ActionResult<PublicCatalogCardPage>> Products(
        [FromQuery] string language = ProductPageRules.Ukrainian,
        [FromQuery] string? type = null,
        [FromQuery] int? sectionId = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(
            new GetPublicCatalogProductsQuery(page, pageSize, language, type, sectionId, category), cancellationToken));

    [HttpGet("products/{slug}", Name = "getPublicCatalogProduct")]
    public async Task<ActionResult<PublicCatalogProduct>> Product(string slug,
        [FromQuery] string language = ProductPageRules.Ukrainian, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogProductQuery(slug, language), cancellationToken));

    [HttpGet("categories", Name = "getPublicCatalogCategories")]
    public async Task<ActionResult<PublicCatalogCategoryTree>> Categories(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogCategoriesQuery(), cancellationToken));

    [HttpGet("categories/{slug}", Name = "getPublicCatalogCategory")]
    public async Task<ActionResult<PublicCatalogCategoryPage>> Category(string slug,
        [FromQuery] string language = ProductPageRules.Ukrainian, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogCategoryQuery(slug, language), cancellationToken));

    [HttpGet("category-slugs", Name = "getPublicCatalogCategorySlugs")]
    public async Task<ActionResult<IReadOnlyList<PublicCatalogSlug>>> CategorySlugs(
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogCategorySlugsQuery(), cancellationToken));

    [HttpGet("product-slugs", Name = "getPublicCatalogProductSlugs")]
    public async Task<ActionResult<IReadOnlyList<PublicCatalogSlug>>> Slugs(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicCatalogProductSlugsQuery(), cancellationToken));
}
