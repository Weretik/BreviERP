using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Templates.Requests;
using Crm.Application.Quotes.Introductions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Templates;

/// <summary>Owned first-contact selections under the shared quote-template namespace.</summary>
[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quote-templates/introductions")]
public sealed class IntroductionTemplatesController(
    IntroductionTemplateService templates, IntroductionQuoteCreator quotes) : ControllerBase
{
    [HttpGet(Name = "getCrmIntroductionTemplates")]
    public async Task<ActionResult> List(bool includeArchived = false, int page = 1, int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100)
            return BadRequest("invalid_pagination");
        var result = await templates.ListAsync(actor, includeArchived, page, pageSize, cancellationToken);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }

    [HttpGet("{templateId:guid}", Name = "getCrmIntroductionTemplate")]
    public async Task<ActionResult> Detail(Guid templateId, Guid? versionId,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (templateId == Guid.Empty || versionId == Guid.Empty) return BadRequest("invalid_template_id");
        var value = await templates.ReadAsync(actor, templateId, versionId, cancellationToken);
        return value is null ? NotFound() : Ok(value);
    }

    [HttpPost(Name = "createCrmIntroductionTemplate")]
    public async Task<ActionResult> Create(IntroductionTemplateWriteRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (request.RequestId == Guid.Empty || request.ExpectedRevision is not null ||
            !request.TryToDomain(out var layout, out var selections))
            return BadRequest("invalid_introduction_template");
        return this.ToActionResult(await templates.CreateAsync(actor, request.RequestId,
            request.Name, layout, request.EmailTemplateVersionId, selections, cancellationToken));
    }

    [HttpPut("{templateId:guid}", Name = "reviseCrmIntroductionTemplate")]
    public async Task<ActionResult> Revise(Guid templateId, IntroductionTemplateWriteRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (templateId == Guid.Empty || request.RequestId == Guid.Empty || request.ExpectedRevision is not > 0 ||
            !request.TryToDomain(out var layout, out var selections))
            return BadRequest("invalid_introduction_template");
        return this.ToActionResult(await templates.ReviseAsync(actor, request.RequestId,
            templateId, request.ExpectedRevision.Value, request.Name, layout,
            request.EmailTemplateVersionId, selections, cancellationToken));
    }

    [HttpPut("{templateId:guid}/archive", Name = "setCrmIntroductionTemplateArchived")]
    public async Task<ActionResult> SetArchived(Guid templateId, IntroductionTemplateArchiveRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (templateId == Guid.Empty || request.RequestId == Guid.Empty || request.ExpectedRevision <= 0)
            return BadRequest("invalid_introduction_template");
        return this.ToActionResult(await templates.SetArchivedAsync(actor, request.RequestId,
            templateId, request.ExpectedRevision, request.Archived, cancellationToken));
    }

    [HttpGet("{templateId:guid}/check", Name = "checkCrmIntroductionTemplate")]
    public async Task<ActionResult> Check(Guid templateId, [FromQuery] Guid? versionId,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (templateId == Guid.Empty || versionId == Guid.Empty) return BadRequest("invalid_template_id");
        return this.ToActionResult(await templates.ApplyAsync(actor, templateId, versionId,
            cancellationToken));
    }

    [HttpPost("{templateId:guid}/apply", Name = "applyCrmIntroductionTemplate")]
    public async Task<ActionResult> Apply(Guid templateId, IntroductionApplyRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (templateId == Guid.Empty || request.RequestId == Guid.Empty || request.VersionId == Guid.Empty ||
            request.CustomerPublicId == Guid.Empty || request.ContactPublicId == Guid.Empty)
            return BadRequest("invalid_introduction_application");
        return this.ToActionResult(await quotes.CreateAsync(actor, request.RequestId,
            templateId, request.VersionId, request.CustomerPublicId, request.ContactPublicId,
            request.RecipientName, cancellationToken));
    }
}
