using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Sewing.Recalculation;
using Catalog.Application.Features.Pricing.Sewing.Recalculation;

namespace Catalog.Api.Controllers.Pricing.Sewing;

/// <summary>«Перерахувати всі» on the «Товари» page and «Перерахувати ціни» of one model (CAT-011).</summary>
[ApiController]
[Route("api/v1/pricing/sewing/recalculation")]
public sealed class SewingPriceRecalculationController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "recalculateSewingPrices")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(SewingRecalculationReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SewingRecalculationReport>> Recalculate(
        [FromBody] RecalculateSewingPricesRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Forbid();
        return this.ToActionResult(await sender.Send(
            new RecalculateSewingPricesCommand(request.ProductIds, request.DryRun, actor), cancellationToken));
    }
}
