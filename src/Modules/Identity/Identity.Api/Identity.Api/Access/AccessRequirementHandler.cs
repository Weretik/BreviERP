using BuildingBlocks.Api.Authorization;
using Identity.Application.Security.Access;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Api.Access;

/// <summary>
/// Checks a request against the user's rights on «Права доступу» (<see cref="AccessCheck"/>): the action is the one the
/// policy names, else the endpoint's <see cref="AccessActionAttribute"/>, else the HTTP method's.
/// </summary>
public sealed class AccessRequirementHandler(IAccessGrantSource grantSource, IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<AccessRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var http = context.Resource as HttpContext ?? httpContextAccessor.HttpContext;
        var named = requirement.Action ?? http?.GetEndpoint()?.Metadata.GetMetadata<AccessActionAttribute>()?.Action;
        if (await AccessCheck.AllowsAsync(grantSource, context.User, requirement.AreaKey, http?.Request.Method ?? HttpMethods.Get,
                AccessCheck.ActionOf(named)))
        {
            context.Succeed(requirement);
        }
    }
}
