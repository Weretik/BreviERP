using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;
using Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

namespace Catalog.Api.Controllers.Pricing.Ppe;

/// <summary>The PPE product card: supplier fields and live three prices (CAT-008).</summary>
[ApiController]
[Route("api/v1/products/{productId:int}/ppe-offer")]
public sealed class PpeSupplierOfferController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getPpeSupplierOffer")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<PpeSupplierOffer>> Get(int productId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPpeSupplierOfferQuery(productId), cancellationToken));

    [HttpPut(Name = "updatePpeSupplierOffer")]
    [Authorize(Policy = "access:products")]
    public async Task<ActionResult<PpeSupplierOffer>> Update(
        int productId, [FromBody] UpdatePpeSupplierOfferRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new UpdatePpeSupplierOfferCommand(productId, request.SupplierId,
            request.SupplierSku, request.SupplierUrl, request.SupplierPrice, request.AllowManualPrice, request.RuleCode,
            request.Unit, request.ExpectedOfferVersionId, actor), cancellationToken));
    }
}
