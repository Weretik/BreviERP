using Catalog.Api.Contracts.Products.Overview;
using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Overview.Update;
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
}
