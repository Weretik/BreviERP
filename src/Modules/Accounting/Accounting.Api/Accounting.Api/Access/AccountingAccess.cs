using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Access;

internal static class AccountingAccess
{
    /// <summary>«Бухгалтерія»: only the Director (user decision 2026-11-05); Admin, Accountant and managers get 403.</summary>
    public const string Policy = "CanUseAccounting";

    /// <summary>E-mail of the signed-in staff account, written to the change log.</summary>
    public static string By(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}

internal static class AccountingResults
{
    /// <summary>204 on success, otherwise the usual mapping (404, 409 with the reason, 400).</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
