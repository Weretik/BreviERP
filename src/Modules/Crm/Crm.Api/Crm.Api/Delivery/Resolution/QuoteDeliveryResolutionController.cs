using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Delivery.ResolveOutcome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Delivery.Resolution;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}/deliveries/{deliveryId:guid}/resolution")]
public sealed class QuoteDeliveryResolutionController(ResolveQuoteDeliveryHandler handler) : ControllerBase
{
    [HttpPost(Name = "resolveCrmQuoteDelivery")]
    public async Task<ActionResult> Resolve(Guid quoteId, Guid deliveryId, ResolveQuoteDeliveryRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await handler.HandleAsync(
            new ResolveQuoteDeliveryCommand(quoteId, deliveryId, request.RequestId, request.ExpectedAttempt,
                request.Outcome, request.EvidenceNote), actor, ct));
    }
}
