using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.Quotes.ArchiveQuote;
using Crm.Application.Quotes.CopyQuote;
using Crm.Application.Quotes.GetQuoteEvents;
using Crm.Application.Quotes.GetQuoteVersion;
using Crm.Application.Quotes.GetQuoteVersions;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.TransferQuote;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}")]
public sealed class QuoteLifecycleController(ISender sender) : ControllerBase
{
    [HttpGet("versions", Name = "getCrmQuoteVersions")]
    public async Task<ActionResult> Versions(Guid quoteId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetQuoteVersionsQuery(quoteId, page, pageSize, actor), cancellationToken);
        return result.IsSuccess ? Ok(new { value = result.Value.Value, pagedInfo = result.Value.PagedInfo })
            : this.ToActionResult(result);
    }

    [HttpGet("versions/{versionId:guid}", Name = "getCrmQuoteVersion")]
    public async Task<ActionResult> Version(Guid quoteId, Guid versionId, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new GetQuoteVersionQuery(quoteId, versionId, actor), cancellationToken));
    }

    [HttpGet("events", Name = "getCrmQuoteEvents")]
    public async Task<ActionResult> Events(Guid quoteId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetQuoteEventsQuery(quoteId, page, pageSize, actor), cancellationToken);
        return result.IsSuccess ? Ok(new { value = result.Value.Value, pagedInfo = result.Value.PagedInfo })
            : this.ToActionResult(result);
    }

    [HttpPost("reprice", Name = "repriceCrmQuote")]
    public async Task<ActionResult> Reprice(Guid quoteId, RepriceQuoteRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new RepriceQuoteCommand(quoteId, request.VersionId, request.ExpectedRevision,
                request.Bases, request.RequestId, actor), cancellationToken));
    }

    [HttpPost("copy", Name = "copyCrmQuote")]
    public async Task<ActionResult> Copy(Guid quoteId, CopyQuoteRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CopyQuoteCommand(quoteId, request.VersionId,
            request.ExpectedRevision, request.RequestId, actor), cancellationToken);
        return result.IsSuccess ? CreatedAtRoute("getCrmQuote", new { quoteId = result.Value.PublicId }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("transfer", Name = "transferCrmQuote")]
    public async Task<ActionResult> Transfer(Guid quoteId, TransferQuoteRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new TransferQuoteCommand(quoteId, request.ExpectedRevision, request.OwnerId,
                request.Reason, request.RequestId, actor), cancellationToken));
    }

    [HttpPut("archive", Name = "setCrmQuoteArchived")]
    public async Task<ActionResult> Archive(Guid quoteId, ArchiveQuoteRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new ArchiveQuoteCommand(quoteId, request.ExpectedRevision, request.Archived,
                request.Reason, request.RequestId, actor), cancellationToken));
    }
}
