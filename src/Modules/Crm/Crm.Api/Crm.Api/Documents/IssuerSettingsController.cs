using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Documents.Requests;
using Crm.Application.Documents.ActivateIssuerSettings;
using Crm.Application.Documents.CreateIssuerSettingsVersion;
using Crm.Application.Documents.GetIssuerSettings;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Documents;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/issuer-settings")]
public sealed class IssuerSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmIssuerSettings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Get(CancellationToken ct)
        => this.ToActionResult(await sender.Send(new GetIssuerSettingsQuery(), ct));

    [HttpPost("versions", Name = "createCrmIssuerSettingsVersion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateVersion(CreateIssuerSettingsVersionRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new CreateIssuerSettingsVersionCommand(request.RequestId, request.Issuer, request.Template, actor), ct));
    }

    [HttpPut("active", Name = "activateCrmIssuerSettings")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Activate(ActivateIssuerSettingsRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new ActivateIssuerSettingsCommand(request.RequestId, request.ExpectedRevision,
                request.IssuerVersionId, request.TemplateVersionId, actor), ct));
    }
}
