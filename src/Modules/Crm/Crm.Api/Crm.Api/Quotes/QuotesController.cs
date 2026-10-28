using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.SaveQuoteVersion;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes")]
public sealed class QuotesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuotes")]
    public async Task<ActionResult> List(
        [FromQuery] string? search, [FromQuery] string? state, [FromQuery] bool? archived,
        [FromQuery] int? ownerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25,
        [FromQuery] DateTimeOffset? createdFrom = null, [FromQuery] DateTimeOffset? createdBefore = null,
        [FromQuery] Guid? sourceQuoteId = null,
        CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100 ||
            search?.Length > 300 || state is not (null or "draft" or "ready" or "sent") ||
            ownerId is <= 0 || sourceQuoteId == Guid.Empty ||
            (createdFrom is not null && createdBefore is not null && createdFrom >= createdBefore))
            return BadRequest("invalid_pagination_or_filter");
        var result = await sender.Send(new GetQuotesQuery(
            actor, search, state, archived, ownerId, page, pageSize, createdFrom, createdBefore, sourceQuoteId), cancellationToken);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }

    [HttpGet("{quoteId:guid}", Name = "getCrmQuote")]
    public async Task<ActionResult> Detail(Guid quoteId, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(
            await sender.Send(new GetQuoteQuery(quoteId, actor), cancellationToken));
    }

    [HttpPost(Name = "createCrmQuote")]
    public async Task<ActionResult> Create(CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateQuoteCommand(
            request.CustomerPublicId, request.ContactPublicId, request.RecipientName,
            request.Content, request.RequestId, actor), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Detail), new { quoteId = result.Value.PublicId }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("price-previews", Name = "previewCrmQuotePrices")]
    public async Task<ActionResult> Preview(
        PreviewQuotePricesRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(
            await sender.Send(new PreviewQuotePricesCommand(
                request.RequestId, request.QuoteId, request.Intent, actor), cancellationToken));
    }

    [HttpPost("{quoteId:guid}/versions", Name = "saveCrmQuoteVersion")]
    public async Task<ActionResult> SaveVersion(
        Guid quoteId, SaveQuoteVersionRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(
            await sender.Send(new SaveQuoteVersionCommand(
                quoteId, request.ExpectedRevision, request.Content, request.RequestId, actor),
                cancellationToken));
    }
}
