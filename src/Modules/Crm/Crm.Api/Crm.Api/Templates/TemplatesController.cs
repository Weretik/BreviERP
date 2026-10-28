using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Templates.Requests;
using Crm.Application.CustomSewing.CreateTemplate;
using Crm.Application.CustomSewing.ReviseTemplate;
using Crm.Application.CustomSewing.ArchiveTemplate;
using Crm.Application.CustomSewing.ApplyTemplate;
using Crm.Application.CustomSewing.GetTemplate;
using Crm.Application.CustomSewing.GetTemplates;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Crm.Api.Templates;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quote-templates")]
public sealed class TemplatesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuoteTemplates")]
    public async Task<ActionResult> List(bool includeArchived = false, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (page is < 1 or > 1000000 || pageSize is < 1 or > 100) return BadRequest("invalid_pagination");
        var result = await sender.Send(new GetTemplatesQuery(actor, includeArchived, page, pageSize), ct);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }
    [HttpGet("{templateId:guid}", Name = "getCrmQuoteTemplate")]
    public async Task<ActionResult> Detail(Guid templateId, Guid? versionId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new GetTemplateQuery(templateId, versionId, actor), ct));
    }
    [HttpPost(Name = "createCrmQuoteTemplate")]
    public async Task<ActionResult> Create(CreateTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (request.Kind != "customConfiguration" || request.Selections is null) return BadRequest("unsupported_template_kind_or_missing_selections");
        return this.ToActionResult(await sender.Send(new CreateTemplateCommand(request.RequestId, request.Introduction, request.Selections.ToDomain(), actor), ct));
    }
    [HttpPut("{templateId:guid}", Name = "reviseCrmQuoteTemplate")]
    public async Task<ActionResult> Revise(Guid templateId, ReviseTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (request.Kind != "customConfiguration" || request.Selections is null) return BadRequest("unsupported_template_kind_or_missing_selections");
        return this.ToActionResult(await sender.Send(new ReviseTemplateCommand(templateId, request.ExpectedRevision, request.RequestId,
            request.Introduction, request.Selections.ToDomain(), actor), ct));
    }
    [HttpPut("{templateId:guid}/archive", Name = "setCrmQuoteTemplateArchived")]
    public async Task<ActionResult> Archive(Guid templateId, ArchiveTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new ArchiveTemplateCommand(templateId,
            request.ExpectedRevision, request.RequestId, request.Archived, actor), ct));
    }
    [HttpPost("{templateId:guid}/apply", Name = "applyCrmQuoteTemplate")]
    public async Task<ActionResult> Apply(Guid templateId, ApplyTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new ApplyTemplateCommand(templateId, request.VersionId,
            request.ExpectedRevision, request.RequestId, request.CustomerPublicId, request.QuotePublicId, actor), ct));
    }
}
