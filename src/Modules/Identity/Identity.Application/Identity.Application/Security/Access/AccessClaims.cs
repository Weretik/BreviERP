using System.Security.Claims;

namespace Identity.Application.Security.Access;

public static class AccessClaims
{
    /// <summary>The role names of a signed-in user, whatever role claim type each identity uses.</summary>
    public static IReadOnlyCollection<string> RolesOf(ClaimsPrincipal user) =>
        user.Identities
            .SelectMany(identity => identity.FindAll(identity.RoleClaimType))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}
