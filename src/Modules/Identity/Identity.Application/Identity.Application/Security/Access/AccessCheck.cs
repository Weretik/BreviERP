using System.Security.Claims;
using Identity.Domain.Authorization;

namespace Identity.Application.Security.Access;

/// <summary>
/// The checks of «Права доступу» that the Api's authorization handler and named policies ask: the director always passes;
/// anyone else needs «Перегляд» on the page and the action the request asks for.
/// </summary>
public static class AccessCheck
{
    /// <summary>
    /// Whether the user may do <paramref name="named"/> (else the HTTP method's action) on the page with
    /// <paramref name="areaKey"/>.
    /// </summary>
    public static async Task<bool> AllowsAsync(IAccessGrantSource grantSource, ClaimsPrincipal user, string areaKey,
        string httpMethod, AccessAction? named, CancellationToken cancellationToken = default)
    {
        var roles = AccessClaims.RolesOf(user);
        if (roles.Any(AccessCatalog.IsLocked)) return true;
        var area = AccessCatalog.Find(areaKey);
        if (area is null) return false;
        var needed = AccessCatalog.Required(area, httpMethod, named);
        var grants = await grantSource.GrantsOfAsync(roles, cancellationToken);
        var allowed = grants.GetValueOrDefault(area.Key);
        return allowed.HasFlag(AccessAction.View) && allowed.HasFlag(needed);
    }

    /// <summary>The action a policy or an endpoint names (<c>view</c>, <c>edit</c>...); null for none or an unknown one.</summary>
    public static AccessAction? ActionOf(string? action) =>
        action is not null && Enum.TryParse<AccessAction>(action, ignoreCase: true, out var parsed) && parsed != AccessAction.None
            ? parsed
            : null;

    /// <summary>One of the given built-in roles, or a role made on «Права доступу».</summary>
    public static bool HasAnyRole(ClaimsPrincipal user, params string[] builtIn) =>
        AccessClaims.RolesOf(user).Any(role =>
            builtIn.Contains(role, StringComparer.Ordinal) || (!AccessCatalog.IsBuiltIn(role) && !AccessCatalog.IsOutsider(role)));
}
