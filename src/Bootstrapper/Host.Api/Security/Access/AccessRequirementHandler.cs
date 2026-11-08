using BuildingBlocks.Api.Authorization;
using Identity.Application.Security.Access;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Host.Api.Security.Access;

/// <summary>
/// Checks a request against the user's rights on «Права доступу»: the director always passes; anyone else needs
/// «Перегляд» on the page and the action the request asks for: the one the policy names, else the endpoint's
/// <see cref="AccessActionAttribute"/>, else the HTTP method's.
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

        var roles = AccessClaims.RolesOf(context.User);
        if (roles.Any(AccessCatalog.IsLocked))
        {
            context.Succeed(requirement);
            return;
        }

        var http = context.Resource as HttpContext ?? httpContextAccessor.HttpContext;
        var named = requirement.Action ?? ActionOf(http?.GetEndpoint()?.Metadata.GetMetadata<AccessActionAttribute>());
        var needed = AccessCatalog.Required(requirement.Area, http?.Request.Method ?? HttpMethods.Get, named);
        var grants = await grantSource.GrantsOfAsync(roles);
        var allowed = grants.GetValueOrDefault(requirement.Area.Key);
        if (allowed.HasFlag(AccessAction.View) && allowed.HasFlag(needed))
        {
            context.Succeed(requirement);
        }
    }

    private static AccessAction? ActionOf(AccessActionAttribute? attribute) =>
        attribute is not null && Enum.TryParse<AccessAction>(attribute.Action, ignoreCase: true, out var action) && action != AccessAction.None
            ? action
            : null;
}
