using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Documents.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Documents.EmailTemplates;
[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/email-templates")]
public sealed class EmailTemplatesController(EmailTemplateService service) : ControllerBase
{
    [HttpGet(Name = "getCrmEmailTemplates")]
    public async Task<ActionResult> List(int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (CrmActorClaims.Read(User) is null) return Forbid();
        if (page is < 1 or > 1000000 || pageSize is < 1 or > 100) return BadRequest("invalid_pagination");
        var result = await service.ListAsync(page, pageSize, ct);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }
    [HttpGet("{versionId:guid}", Name = "getCrmEmailTemplateVersion")]
    public async Task<ActionResult> Detail(Guid versionId, CancellationToken ct)
    {
        if (CrmActorClaims.Read(User) is null) return Forbid();
        if (versionId == Guid.Empty) return BadRequest("invalid_version_id");
        var result = await service.ReadAsync(versionId, ct);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpGet("active", Name = "getCrmActiveEmailTemplate")]
    public async Task<ActionResult> Active(CancellationToken ct)
        => CrmActorClaims.Read(User) is null ? Forbid() : Ok(await service.ActiveAsync(ct));

    [HttpPost("versions", Name = "createCrmEmailTemplateVersion")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(CreateEmailTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await service.CreateAsync(actor,
            request.RequestId, request.Name, request.Subject, request.Body, ct));
    }
    [HttpPut("active", Name = "activateCrmEmailTemplate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Activate(ActivateEmailTemplateRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await service.ActivateAsync(actor,
            request.RequestId, request.ExpectedRevision, request.VersionId, ct));
    }
}
