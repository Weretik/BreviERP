using Identity.Api.Contracts.Access;
using Identity.Application.Security.Policies;
using Identity.Application.Features.Access.Commands.CreateRole;
using Identity.Application.Features.Access.Commands.DeleteRole;
using Identity.Application.Features.Access.Commands.SaveRoleAccess;
using Identity.Application.Features.Access.Queries.GetAccessRights;

namespace Identity.Api.Controllers;

/// <summary>
/// «Адміністрування → Права доступу»: which pages each role sees and what it may do there, plus new roles.
/// The director always has everything (400 on save); built-in roles are never deleted (400); a role with users is not
/// deleted (409); a save over someone else's newer save is refused (409).
/// </summary>
[ApiController]
[Route("api/admin/access")]
[Authorize(Policy = PolicyNames.CanManageUsers)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AccessRightsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAccessRights")]
    [ProducesResponseType(typeof(AccessRightsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessRightsResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccessRightsQuery(), cancellationToken);
        return Ok(AccessRightsResponse.From(result.Value));
    }

    [HttpPut("roles/{role}", Name = "saveRoleAccess")]
    [ProducesResponseType(typeof(AccessRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessRoleResponse>> Save(
        string role,
        [FromBody] SaveRoleAccessRequest request,
        CancellationToken cancellationToken)
    {
        var grants = AccessActionNames.Parse(request.Grants);
        if (grants is null)
        {
            ModelState.AddModelError(nameof(request.Grants), "Actions are view, create, edit and delete.");
            return ValidationProblem(ModelState);
        }

        var result = await sender.Send(new SaveRoleAccessCommand(role, request.Version, grants), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Ok(AccessRoleResponse.From(result.Value))
            : this.ToActionResult(result);
    }

    [HttpPost("roles", Name = "createRole")]
    [ProducesResponseType(typeof(AccessRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessRoleResponse>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRoleCommand(request.Title ?? string.Empty), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? StatusCode(StatusCodes.Status201Created, AccessRoleResponse.From(result.Value))
            : this.ToActionResult(result);
    }

    [HttpDelete("roles/{role}", Name = "deleteRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string role, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRoleCommand(role), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? NoContent() : this.ToActionResult(result);
    }
}
