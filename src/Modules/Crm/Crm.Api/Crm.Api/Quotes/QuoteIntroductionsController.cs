using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;
using Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Api.Quotes;

/// <summary>«КП для знайомства»: a catalogue of season pages, 4 models each (R-16–R-18).
/// It is read, listed, sent and deleted through <c>api/crm/quotes</c>.</summary>
[ApiController]
[Authorize(Policy = CrmAccess.Quotes)]
[Route("api/crm/quote-introductions")]
public sealed class QuoteIntroductionsController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "createCrmQuoteIntroduction")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Create(SaveQuoteIntroductionInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateQuoteIntroductionCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Created($"/api/crm/quotes/{result.Value.Id}", result.Value)
            : this.ToActionResult(result);
    }

    /// <summary>Saves the whole introduction; 409 when someone saved a newer version.</summary>
    [HttpPut("{id:long}", Name = "saveCrmQuoteIntroduction")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Save(long id, SaveQuoteIntroductionInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SaveQuoteIntroductionCommand(actor, id, input),
            cancellationToken));
    }
}
