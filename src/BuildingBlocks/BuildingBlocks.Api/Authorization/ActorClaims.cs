using System.Security.Claims;

namespace BuildingBlocks.Api.Authorization;

public static class ActorClaims
{
    /// <summary>Who made the change, as the modules record it: e-mail, else user name, else user id.</summary>
    public static string ActorName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
