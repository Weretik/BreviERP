using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Sewing.Calculations;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Calculations.Get;
using Catalog.Application.Features.Pricing.Sewing.Publications.GetCurrent;
using Catalog.Application.Features.Pricing.Sewing.Publications.Publish;
using Catalog.Application.Features.Pricing.Sewing.Calculations.Save;

namespace Catalog.Api.Controllers.Pricing.Sewing;

[ApiController]
[Route("api/v1/products/{productId:int}/articles/{articleId:long}/pricing")]
public sealed class SewingPricingController(ISender sender) : ControllerBase
{
    [HttpPost("calculations", Name = "saveSewingCalculation")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<SavedSewingCalculation>> Save(
        int productId, long articleId, [FromBody] SaveSewingCalculationRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Forbid();
        var result = await sender.Send(new SaveSewingCalculationCommand(productId, articleId, request.RequestId, actor), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("calculations/{calculationId:guid}", Name = "getSewingCalculation")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<SavedSewingCalculation>> Get(
        int productId, long articleId, Guid calculationId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSewingCalculationQuery(productId, articleId, calculationId), cancellationToken));

    [HttpPost("calculations/{calculationId:guid}/publication", Name = "publishSewingCalculation")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<PublishedSewingPrice>> Publish(
        int productId, long articleId, Guid calculationId, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Forbid();
        var result = await sender.Send(new PublishSewingCalculationCommand(productId, articleId, calculationId, actor), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("published", Name = "getPublishedSewingPrice")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<PublishedSewingPrice>> GetPublished(
        int productId, long articleId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPublishedSewingPriceQuery(productId, articleId), cancellationToken));
}
