using System.Security.Claims;

namespace Accounting.Api.Access;

internal static class AccountingAccess
{
    /// <summary>
    /// «Бухгалтерія» on «Права доступу»: only the Director until someone hands it out (user decision 2026-11-05).
    /// </summary>
    public const string Policy = "access:accounting";

    /// <summary>E-mail of the signed-in staff account, written to the change log.</summary>
    public static string By(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
