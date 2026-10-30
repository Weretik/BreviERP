using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.GetDeliveries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Delivery;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}/deliveries")]
public sealed class QuoteDeliveriesController(EnqueueQuoteDeliveryHandler enqueue, QuoteDeliveryReader reader) : ControllerBase
{
    [HttpPost(Name = "enqueueCrmQuoteDelivery")]
    public async Task<ActionResult> Enqueue(Guid quoteId, EnqueueQuoteDeliveryRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await enqueue.HandleAsync(new EnqueueQuoteDeliveryCommand(quoteId, request.RequestId,
            request.ExpectedRevision, request.VersionId, request.ArtifactId, request.EmailTemplateVersionId,
            request.RecipientAddress, request.ManagerName), actor, ct);
        if (result.Status == ResultStatus.Unavailable) return StatusCode(503, new { errors = result.Errors });
        return result.IsSuccess ? StatusCode(202, result.Value) : this.ToActionResult(result);
    }

    [HttpGet(Name = "getCrmQuoteDeliveries")]
    public async Task<ActionResult> List(Guid quoteId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await reader.ListAsync(quoteId, actor, page, pageSize, ct);
        return result.IsSuccess ? Ok(new { value = result.Value.Value, pagedInfo = result.Value.PagedInfo }) : this.ToActionResult(result);
    }

    [HttpGet("{deliveryId:guid}", Name = "getCrmQuoteDelivery")]
    public async Task<ActionResult> Get(Guid quoteId, Guid deliveryId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await reader.GetAsync(quoteId, deliveryId, actor, ct));
    }
}
