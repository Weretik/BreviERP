using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.DeleteQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.Pdf.GetPdf;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.SaveQuote;
using Crm.Application.Quotes.SetResponsible;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

/// <summary>КП (commercial proposals) of the «Менеджер сервісу» block. A Manager sees only own КП.</summary>
[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes")]
public sealed class QuotesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuotes")]
    [ProducesResponseType<QuotePageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuotePageResponse>> List([FromQuery] string? search, [FromQuery] string? status,
        [FromQuery] int? responsibleUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new GetQuotesQuery(actor, search, status, responsibleUserId, page, pageSize), cancellationToken));
    }

    [HttpGet("{id:long}", Name = "getCrmQuote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Detail(long id, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetQuoteQuery(actor, id), cancellationToken));
    }

    /// <summary>«Сформувати КП»: the PDF of the saved КП, only what the client may see (R-14).</summary>
    [HttpGet("{id:long}/pdf", Name = "getCrmQuotePdf")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pdf(long id, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetQuotePdfQuery(actor, id), cancellationToken);
        if (!result.IsSuccess) return this.ToActionResult(result);
        Response.Headers.CacheControl = "no-store";
        return File(result.Value.Content, "application/pdf", result.Value.FileName);
    }

    [HttpPost(Name = "createCrmQuote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Create(SaveQuoteInput input, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateQuoteCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Detail), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    /// <summary>Saves the whole editor; 409 when someone saved a newer version.</summary>
    [HttpPut("{id:long}", Name = "saveCrmQuote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Save(long id, SaveQuoteInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SaveQuoteCommand(actor, id, input), cancellationToken));
    }

    [HttpDelete("{id:long}", Name = "deleteCrmQuote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(long id, [FromQuery] int version, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new DeleteQuoteCommand(actor, id, version), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    /// <summary>«Перерахувати за актуальними цінами»: preview with apply=false, then apply=true.</summary>
    [HttpPost("{id:long}/reprice", Name = "repriceCrmQuote")]
    [ProducesResponseType<QuoteRepriceReport>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteRepriceReport>> Reprice(long id, RepriceQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new RepriceQuoteCommand(actor, id, request.Version, request.Apply), cancellationToken));
    }

    /// <summary>Director or Admin gives the КП to a manager.</summary>
    [HttpPut("{id:long}/responsible", Name = "setCrmQuoteResponsible")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> SetResponsible(long id, SetQuoteResponsibleRequest request,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new SetQuoteResponsibleCommand(actor, id, request.Version, request.UserId), cancellationToken));
    }
}
