using System.Security.Claims;
using Catalog.Api.Contracts.Products.Overview;
using Catalog.Application.Features.Product.Overview.CodeCheck;
using Catalog.Application.Features.Product.Overview.Create;
using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Overview.Update;
using Catalog.Application.Features.Product.SheetImport;
using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Api.Controllers;

/// <summary>The «Товари» page: one row per model (sewing) or site article (PPE) with its working fields.</summary>
[ApiController]
[Route("api/v1/products")]
public sealed class ProductOverviewController(ISender sender) : ControllerBase
{
    [HttpGet("overview")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductOverviewRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductOverviewRow>>> Get(
        [FromQuery] string? type, CancellationToken cancellationToken)
    {
        ProductType? productType = null;
        if (!string.IsNullOrEmpty(type))
        {
            if (!ProductOverviewTypes.TryParse(type, out var parsed))
                return BadRequest("type must be Sewing or Ppe.");
            productType = parsed;
        }

        return this.ToActionResult(await sender.Send(new GetProductOverviewQuery(productType), cancellationToken));
    }

    [HttpPut("{id:int}/overview")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id, [FromBody] UpdateProductOverviewRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductOverviewCommand(id, request.Code, request.SectionId,
            request.Season, request.IsActive, request.IsVerified, request.VerifiedOn, request.CurrentSiteUrl,
            request.QuoteDescription), cancellationToken));

    [HttpPost("overview", Name = "createCatalogProduct")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(CreatedCatalogProduct), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedCatalogProduct>> Create(
        [FromBody] CreateCatalogProductRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        var ppe = request.Ppe is { } p
            ? new CreatePpeSupplierInput(p.SupplierId, p.SupplierSku, p.SupplierUrl, p.SupplierPrice,
                p.AllowManualPrice, p.RuleCode, p.Unit)
            : null;
        var result = await sender.Send(new CreateCatalogProductCommand(request.Type, request.Code, request.Name,
            request.SectionId, request.Season, request.IsActive, request.IsVerified, request.CurrentSiteUrl,
            request.QuoteDescription, ppe, actor), cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    /// <summary>
    /// One-time import of all products from the Calculator sheet (CAT-010): dryRun returns the report; otherwise the
    /// products are saved in one transaction when the report has no problem. With deleteMissing, products that are
    /// not in the sheet are deleted.
    /// </summary>
    [HttpPost("sheet-import", Name = "importSheetProducts")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(SheetImportReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SheetImportReport>> ImportSheet(
        [FromBody] ImportSheetProductsRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new ImportSheetProductsCommand(request.Sewing, request.Ppe,
            request.DryRun, request.DeleteMissing, actor), cancellationToken));
    }

    [HttpGet("overview/code-check", Name = "getProductCodeCheck")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(ProductCodeCheck), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductCodeCheck>> CheckCode(
        [FromQuery] string type, [FromQuery] string? code, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductCodeCheckQuery(type, code), cancellationToken));
}
