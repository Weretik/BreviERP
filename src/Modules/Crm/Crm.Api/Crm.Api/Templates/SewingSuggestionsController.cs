using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.CustomSewing.GetSuggestions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Crm.Api.Templates;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/custom-sewing/suggestions")]
public sealed class SewingSuggestionsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmSewingSuggestions")]
    public async Task<ActionResult> Read([FromQuery] string productType, [FromQuery] string season, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new GetSuggestionsQuery(productType, season, actor), ct));
    }
}
