using Identity.Application.Features.Access.Queries.GetMyAccess;
using Identity.Application.Features.Auth.Queries.SessionMe;
using Identity.Api.Access;

namespace Identity.Api.Auth;

/// <summary>
/// The signed-in user of the current browser session.
/// </summary>
[ApiController]
[Route("api/auth/session")]
public sealed class AuthSessionController(ISender sender) : ControllerBase
{
    [HttpGet("me", Name = "getSessionMe")]
    [Authorize]
    [ProducesResponseType(typeof(SessionMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionMeResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionMeQuery(), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(SessionMeResponse.From(result.Value));
    }

    /// <summary>
    /// What the signed-in user may do, by page of «Права доступу»: the admin hides menu items and buttons by it; the
    /// server checks the same rights on every request.
    /// </summary>
    [HttpGet("access", Name = "getSessionAccess")]
    [Authorize]
    [ProducesResponseType(typeof(MyAccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MyAccessResponse>> Access(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyAccessQuery(), cancellationToken);
        return Ok(MyAccessResponse.From(result.Value));
    }
}
