using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;

namespace Crm.Api.Quotes;

/// <summary>
/// One-time transfer of КП from the old calculator (R-25): Director and Admin only. Run a dry run first; the real run
/// saves every new КП in one transaction and skips the ones transferred before.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/crm/quote-import")]
public sealed class QuoteImportController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "importCrmQuotes")]
    [ProducesResponseType<QuoteImportReport>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteImportReport>> Import(ImportQuotesRequest request,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new ImportQuotesCommand(actor, request.DryRun, request.Quotes),
            cancellationToken));
    }
}
