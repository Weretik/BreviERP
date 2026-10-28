using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Quotes.GetQuoteAssignees;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quote-assignees")]
public sealed class QuoteAssigneesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuoteAssignees")]
    public async Task<ActionResult> Search([FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetQuoteAssigneesQuery(search, page, pageSize, actor), cancellationToken);
        return result.IsSuccess ? Ok(new { value = result.Value.Value, pagedInfo = result.Value.PagedInfo })
            : this.ToActionResult(result);
    }
}
