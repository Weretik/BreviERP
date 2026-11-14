using System.Security.Claims;
using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;
using Crm.Application.Features.SiteRequests.Queries.GetSiteRequests;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Api.SiteRequests;

/// <summary>
/// «Менеджер сервісу › Заявки з сайту» (LEAD-002): requests of the public site. Everyone with the page sees them and
/// takes a free one; the owner or an Admin closes or frees it.
/// </summary>
[ApiController]
[Authorize(Policy = CrmAccess.SiteRequests)]
[Route("api/crm/site-requests")]
public sealed class SiteRequestsController(ISender sender) : ControllerBase
{
    /// <param name="scope">`Open` (default), `Mine`, `Closed` or `All`.</param>
    [HttpGet(Name = "getCrmSiteRequests")]
    [ProducesResponseType<SiteRequestPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SiteRequestPageResponse>> List([FromQuery] string? scope,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        if (UserId() is not { } userId) return Forbid();
        return this.ToActionResult(await sender.Send(new GetSiteRequestsQuery(userId, scope, page, pageSize),
            cancellationToken));
    }

    /// <summary>«Беру», «Готово», «Не заявка», «Повернути в нові»; 409 when the version is stale or it is taken.</summary>
    [HttpPost("{id:long}/change", Name = "changeCrmSiteRequest")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<SiteRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SiteRequestResponse>> Change(long id, ChangeSiteRequestInput input,
        CancellationToken cancellationToken)
    {
        if (UserId() is not { } userId) return Forbid();
        return this.ToActionResult(await sender.Send(
            new ChangeSiteRequestCommand(userId, User.IsInRole("Admin"), id, input), cancellationToken));
    }

    private int? UserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;
}
