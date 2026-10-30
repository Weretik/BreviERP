using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.CustomSewing.Requests;
using Crm.Application.CustomSewing.CreateDraft;
using Crm.Application.CustomSewing.SaveDraft;
using Crm.Application.CustomSewing.Calculate;
using Crm.Application.CustomSewing.SaveVersion;
using Crm.Application.CustomSewing.GetDraft;
using Crm.Application.CustomSewing.GetVersion;
using Crm.Application.CustomSewing.GetVersions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.CustomSewing;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/custom-sewing")]
public sealed class CustomSewingController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "createCrmCustomSewingDraft")]
    public async Task<ActionResult> Create(CreateCustomDraftRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateCustomDraftCommand(request.RequestId, request.CustomerPublicId,
            request.QuotePublicId, request.Configuration.ToDomain(), actor), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(Detail), new { draftId = result.Value.PublicId }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{draftId:guid}", Name = "getCrmCustomSewingDraft")]
    public async Task<ActionResult> Detail(Guid draftId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new GetCustomDraftQuery(draftId, actor), ct));
    }

    [HttpPut("{draftId:guid}", Name = "saveCrmCustomSewingDraft")]
    public async Task<ActionResult> Save(Guid draftId, SaveCustomDraftRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new SaveCustomDraftCommand(draftId,
            request.ExpectedRevision, request.RequestId, request.Configuration.ToDomain(), actor), ct));
    }

    [HttpPost("{draftId:guid}/calculations", Name = "calculateCrmCustomSewing")]
    public async Task<ActionResult> Calculate(Guid draftId, CalculateCustomSewingRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new CalculateCustomSewingCommand(draftId,
            request.ExpectedRevision, request.RequestId, actor), ct));
    }

    [HttpPost("{draftId:guid}/versions", Name = "saveCrmCustomSewingVersion")]
    public async Task<ActionResult> SaveVersion(Guid draftId, SaveCustomVersionRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new SaveCustomVersionCommand(draftId,
            request.ExpectedRevision, request.RequestId, request.BasisToken, actor), ct));
    }

    [HttpGet("{draftId:guid}/versions", Name = "getCrmCustomSewingVersions")]
    public async Task<ActionResult> Versions(Guid draftId, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (page is < 1 or > 1000000 || pageSize is < 1 or > 100) return BadRequest("invalid_pagination");
        var accessible = await sender.Send(new GetCustomDraftQuery(draftId, actor), ct);
        if (!accessible.IsSuccess) return this.ToActionResult(accessible);
        var result = await sender.Send(new GetCustomVersionsQuery(draftId, page, pageSize, actor), ct);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }

    [HttpGet("{draftId:guid}/versions/{versionId:guid}", Name = "getCrmCustomSewingVersion")]
    public async Task<ActionResult> Version(Guid draftId, Guid versionId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new GetCustomVersionQuery(draftId, versionId, actor), ct));
    }
}
