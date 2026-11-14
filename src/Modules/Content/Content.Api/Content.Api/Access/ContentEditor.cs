using System.Security.Claims;

namespace Content.Api.Access;

/// <summary>Who saved a site text: e-mail, else name, else id of the signed-in staff member.</summary>
internal static class ContentEditor
{
    public static string By(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email)
        ?? user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
