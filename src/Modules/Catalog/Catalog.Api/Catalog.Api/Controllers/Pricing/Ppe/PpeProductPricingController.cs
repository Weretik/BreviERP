using BuildingBlocks.Api.Authorization;
using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
using Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
using Catalog.Application.Features.Pricing.Shared;
namespace Catalog.Api.Controllers.Pricing.Ppe;
[ApiController]
[Route("api/v1/pricing/ppe/products")]
public sealed class PpeProductPricingController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getPpeProductPrices")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<IReadOnlyList<PpeProductPricing>>> Batch([FromQuery] int[] productIds, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPpeProductPricesQuery(productIds), cancellationToken));

    [HttpPost("{productId:int}/offer/versions", Name = "createPpeOfferVersion")]
    [AccessAction(AccessActionAttribute.Edit)]
    [Authorize(Policy = "access:products")]
    public async Task<ActionResult<PpeOfferVersion>> Create(int productId, [FromBody] CreatePpeOfferVersionRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new CreatePpeOfferVersionCommand(productId,
            request.ExpectedVersionId, request.RequestId, request.Values.ToValues(), actor), cancellationToken));
    }
}
