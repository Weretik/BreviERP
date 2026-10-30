using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.Quotes.PrepareQuote;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}/ready")]
public sealed class QuoteReadyController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "prepareCrmQuote")]
    public async Task<ActionResult> Prepare(Guid quoteId, PrepareQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new PrepareQuoteCommand(quoteId, request.VersionId, request.ExpectedRevision,
                request.RequestId, actor), cancellationToken));
    }
}
