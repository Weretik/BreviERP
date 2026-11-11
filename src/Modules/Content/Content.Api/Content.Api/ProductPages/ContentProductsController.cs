using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Contracts;
using Content.Application.ProductPages.Delete;
using Content.Application.ProductPages.GetList;
using Content.Application.ProductPages.GetOne;
using Content.Application.ProductPages.ImportOldSite;
using Content.Application.ProductPages.Save;
using Content.Application.ProductPages.SetVisibility;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.ProductPages;

/// <summary>«Контент → Товари»: site pages of Catalog products (CNT-002).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.ProductsPolicy)]
[Route("api/content/products")]
public sealed class ContentProductsController(ISender sender) : ControllerBase
{
    public const int DefaultPageSize = 50;

    [HttpGet(Name = "getContentProducts")]
    public async Task<ActionResult<ContentProductPage>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] int? sectionId = null,
        [FromQuery] bool? onSite = null,
        [FromQuery] int? categoryId = null,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(
            new GetContentProductsQuery(page, pageSize, search, type, sectionId, onSite, categoryId), cancellationToken));

    [HttpGet("{productId:int}", Name = "getContentProduct")]
    public async Task<ActionResult<ContentProductResponse>> Get(int productId, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetContentProductQuery(productId), cancellationToken));

    /// <summary>Creates or replaces the page: 409 when the revision is stale or the address is taken.</summary>
    [HttpPut("{productId:int}", Name = "saveContentProduct")]
    public async Task<ActionResult<ContentProductResponse>> Save(int productId,
        [FromBody] SaveContentProductRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SaveContentProductCommand(productId, request.Revision, request.Slug, request.ShowOnSite, request.Uk,
                request.Ru, By(), request.CategoryIds), cancellationToken));

    /// <summary>«Показувати на сайті» from the list.</summary>
    [HttpPatch("{productId:int}/visibility", Name = "setContentProductVisibility")]
    public async Task<ActionResult<ContentProductVisibilityResponse>> SetVisibility(int productId,
        [FromBody] ContentProductVisibilityRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SetContentProductVisibilityCommand(productId, request.ShowOnSite, By()), cancellationToken));

    /// <summary>
    /// One-time transfer of old-site texts and SEO; needs the create right. Counts only unless <c>dryRun</c> is false.
    /// </summary>
    [HttpPost("old-site-import", Name = "importOldSiteProductPages")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<OldSiteImportResponse>> ImportOldSite(
        [FromBody] ImportOldSiteProductPagesRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new ImportOldSiteProductPagesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                By()), cancellationToken));

    /// <summary>Removes the site page; the Catalog product stays.</summary>
    [HttpDelete("{productId:int}", Name = "deleteContentProduct")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentProductCommand(productId), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
