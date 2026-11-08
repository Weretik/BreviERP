using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Introductions.Templates.CreateTemplate;
using Crm.Application.Quotes.Introductions.Templates.DeleteTemplate;
using Crm.Application.Quotes.Introductions.Templates.GetTemplates;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

/// <summary>«Готові шаблони» of «КП для знайомства»: shared by all staff, no prices (R-19).</summary>
[ApiController]
[Authorize(Policy = CrmAccess.Quotes)]
[Route("api/crm/quote-intro-templates")]
public sealed class QuoteIntroTemplatesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuoteIntroTemplates")]
    [ProducesResponseType<IReadOnlyList<QuoteIntroTemplateResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuoteIntroTemplateResponse>>> List(
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetQuoteIntroTemplatesQuery(actor), cancellationToken));
    }

    [HttpPost(Name = "createCrmQuoteIntroTemplate")]
    [ProducesResponseType<QuoteIntroTemplateResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteIntroTemplateResponse>> Create(CreateQuoteIntroTemplateInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateQuoteIntroTemplateCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Created($"/api/crm/quote-intro-templates/{result.Value.Id}", result.Value)
            : this.ToActionResult(result);
    }

    /// <summary>The author, Director or Admin.</summary>
    [HttpDelete("{id:long}", Name = "deleteCrmQuoteIntroTemplate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new DeleteQuoteIntroTemplateCommand(actor, id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }
}
