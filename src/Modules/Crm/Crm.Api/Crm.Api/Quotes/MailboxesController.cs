using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Features.Quotes.Commands.CompleteMailboxConnect;
using Crm.Application.Features.Quotes.Commands.DisconnectMailbox;
using Crm.Application.Features.Quotes.Commands.StartMailboxConnect;

namespace Crm.Api.Quotes;

/// <summary>
/// Gmail of staff for sending КП (R-15). The Director or Admin connects anyone, a manager only themselves; the person
/// signs in to their Gmail on the Google page and allows sending only.
/// </summary>
[ApiController]
[Authorize(Policy = CrmAccess.QuotesView)]
[Route("api/crm/mailboxes")]
public sealed class MailboxesController(ISender sender) : ControllerBase
{
    /// <summary>The Google consent address; the Admin opens it in the same tab.</summary>
    [HttpPost("{userId:int}/connect", Name = "connectCrmMailbox")]
    [ProducesResponseType<MailboxConnectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MailboxConnectResponse>> Connect(int userId, MailboxConnectRequest request,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new StartMailboxConnectCommand(actor, userId, request.ReturnPath), cancellationToken));
    }

    [HttpDelete("{userId:int}", Name = "disconnectCrmMailbox")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Disconnect(int userId, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new DisconnectMailboxCommand(actor, userId), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    /// <summary>Google returns the browser here; it always redirects back to the Admin with the outcome.</summary>
    [HttpGet("google-callback", Name = "crmMailboxGoogleCallback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<ActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? state,
        [FromQuery] string? error, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteMailboxConnectCommand(code, state, error), cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return result.IsSuccess ? Redirect(result.Value) : BadRequest();
    }
}
