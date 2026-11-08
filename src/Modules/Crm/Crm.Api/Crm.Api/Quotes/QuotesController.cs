using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.Quotes.AddEmbroidery;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CopyQuote;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.DeleteQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.History;
using Crm.Application.Quotes.LegacyArchive;
using Crm.Application.Quotes.Pdf.GetPdf;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.SaveQuote;
using Crm.Application.Quotes.Sending.Draft;
using Crm.Application.Quotes.Sending.Send;
using Crm.Application.Quotes.SetResponsible;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

/// <summary>КП (commercial proposals) of the «Менеджер сервісу» block. A Manager sees only own КП.</summary>
[ApiController]
[Authorize(Policy = CrmAccess.Quotes)]
[Route("api/crm/quotes")]
public sealed class QuotesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuotes")]
    [ProducesResponseType<QuotePageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuotePageResponse>> List([FromQuery] string? search, [FromQuery] string? status,
        [FromQuery] int? responsibleUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] long? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new GetQuotesQuery(actor, search, status, responsibleUserId, page, pageSize, customerId),
            cancellationToken));
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

    /// <summary>«Видалити КП»: the «Видаляти» right of «Права доступу» (the Director by default); a КП with a production order stays (409).</summary>
    [HttpDelete("{id:long}", Name = "deleteCrmQuote")]
    [Authorize(Policy = CrmAccess.QuotesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(long id, [FromQuery] int version, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User, canDelete: true);
        if (actor is null) return Forbid();
        var result = await sender.Send(new DeleteQuoteCommand(actor, id, version), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    /// <summary>«Перерахувати за актуальними цінами»: preview with apply=false, then apply=true.</summary>
    [HttpPost("{id:long}/reprice", Name = "repriceCrmQuote")]
    [AccessAction(AccessActionAttribute.Edit)]
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

    /// <summary>«Надіслати КП»: the sender's Gmail and the letters with marks filled (R-15).</summary>
    [HttpGet("{id:long}/send-draft", Name = "getCrmQuoteSendDraft")]
    [ProducesResponseType<QuoteSendDraftResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteSendDraftResponse>> SendDraft(long id, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetQuoteSendDraftQuery(actor, id), cancellationToken));
    }

    /// <summary>Sends the PDF of the saved КП from the signed-in person's Gmail; 409 carries the reason of a failure.</summary>
    [HttpPost("{id:long}/send", Name = "sendCrmQuote")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Send(long id, SendQuoteInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SendQuoteCommand(actor, id, input), cancellationToken));
    }

    /// <summary>«Історія змін»: who created, saved, sent, repriced, passed, archived or copied the КП.</summary>
    [HttpGet("{id:long}/history", Name = "getCrmQuoteHistory")]
    [ProducesResponseType<IReadOnlyList<QuoteEventResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<QuoteEventResponse>>> History(long id,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetQuoteHistoryQuery(actor, id), cancellationToken));
    }

    /// <summary>How many КП are left from the removed «Архів»; the Director sees it before deleting them.</summary>
    [HttpGet("legacy-archive", Name = "getCrmLegacyArchivedQuotes")]
    [Authorize(Policy = CrmAccess.QuotesDelete)]
    [ProducesResponseType<LegacyArchivedQuotesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LegacyArchivedQuotesResponse>> LegacyArchive(CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User, canDelete: true);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetLegacyArchivedQuotesQuery(actor), cancellationToken));
    }

    /// <summary>Deletes the КП left from the removed «Архів»; one with a production order returns to the list.</summary>
    [HttpDelete("legacy-archive", Name = "purgeCrmLegacyArchivedQuotes")]
    [Authorize(Policy = CrmAccess.QuotesDelete)]
    [ProducesResponseType<LegacyArchivePurgeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LegacyArchivePurgeResponse>> PurgeLegacyArchive(CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User, canDelete: true);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new PurgeLegacyArchivedQuotesCommand(actor), cancellationToken));
    }

    /// <summary>«Додати в КП» from «Ціна вишивки»: appends the saved calculation as one item; no version needed.</summary>
    [HttpPost("{id:long}/embroidery", Name = "addCrmQuoteEmbroidery")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> AddEmbroidery(long id, AddQuoteEmbroideryRequest request,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new AddEmbroideryToQuoteCommand(actor, id, request.CalculationId), cancellationToken));
    }

    /// <summary>«Копіювати»: a new draft with the next number and the same items and saved prices.</summary>
    [HttpPost("{id:long}/copy", Name = "copyCrmQuote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Copy(long id, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CopyQuoteCommand(actor, id), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Detail), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }
}
