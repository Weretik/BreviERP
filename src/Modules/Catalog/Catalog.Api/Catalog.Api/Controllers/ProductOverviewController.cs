using System.Security.Claims;
using Catalog.Api.Contracts.Products.Overview;
using Catalog.Domain.Products;
using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts;
using Catalog.Application.Features.Product.Commands.UpdateProductOverview;
using Catalog.Application.Features.Product.Queries.GetProductCodeCheck;
using Catalog.Application.Features.Product.Queries.GetProductOverviewItem;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Shared.Overview;

namespace Catalog.Api.Controllers;

/// <summary>The «Товари» page: one row per model (sewing) or site article (PPE) with its working fields.</summary>
[ApiController]
[Route("api/v1/products")]
public sealed class ProductOverviewController(ISender sender) : ControllerBase
{
    /// <summary>
    /// One page of a «Товари» tab (CAT-013): searched, filtered, sorted and paged on the server, with published prices
    /// and fabric variants. section is a section ID or "none"; active=true/false shows only active or inactive rows.
    /// </summary>
    [HttpGet("overview", Name = "getProductOverview")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(ProductOverviewPage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductOverviewPage>> Get(
        [FromQuery] string? type, [FromQuery] string? search, [FromQuery] string? section,
        [FromQuery] bool withoutPrice = false, [FromQuery] string? sort = null, [FromQuery] string? direction = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] bool? active = null,
        CancellationToken cancellationToken = default)
    {
        if (!ProductOverviewTypes.TryParse(type, out var productType))
            return BadRequest("type must be Sewing or Ppe.");
        int? sectionId = null;
        var withoutSection = string.Equals(section, "none", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(section) && !withoutSection)
        {
            if (!int.TryParse(section, out var parsedSection))
                return BadRequest("section must be a section ID or none.");
            sectionId = parsedSection;
        }

        if (!Enum.TryParse<ProductOverviewSort>(sort ?? nameof(ProductOverviewSort.Code), true, out var order)
            || !Enum.IsDefined(order) || int.TryParse(sort, out _))
            return BadRequest("sort must be code, name, price or updated.");
        if (direction is not (null or "asc" or "desc"))
            return BadRequest("direction must be asc or desc.");

        return this.ToActionResult(await sender.Send(new GetProductOverviewPageQuery(productType, search, sectionId,
            withoutSection, withoutPrice, order, direction == "desc", page, pageSize, active), cancellationToken));
    }

    /// <summary>One «Товари» row with its prices and fabric variants, for the product card (CAT-013).</summary>
    [HttpGet("{id:int}/overview", Name = "getProductOverviewItem")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(ProductOverviewItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductOverviewItem>> GetOne([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductOverviewItemQuery(id), cancellationToken));

    [HttpPut("{id:int}/overview")]
    [Authorize(Policy = "access:products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id, [FromBody] UpdateProductOverviewRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateProductOverviewCommand(id, request.Code, request.SectionId,
            request.Season, request.IsActive, request.CurrentSiteUrl,
            request.QuoteDescription), cancellationToken));

    [HttpPost("overview", Name = "createCatalogProduct")]
    [Authorize(Policy = "access:products")]
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
            request.SectionId, request.Season, request.IsActive, request.CurrentSiteUrl,
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
    [Authorize(Policy = "access:products")]
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
