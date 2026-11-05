using System.Security.Claims;

namespace Accounting.Api.Access;

internal static class AccountingAccess
{
    /// <summary>«Бухгалтерія»: only the Director (user decision 2026-11-05); Admin, Accountant and managers get 403.</summary>
    public const string Policy = "CanUseAccounting";

    /// <summary>E-mail of the signed-in staff account, written to the change log.</summary>
    public static string By(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
