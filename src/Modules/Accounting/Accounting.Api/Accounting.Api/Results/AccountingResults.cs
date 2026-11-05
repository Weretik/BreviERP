using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Results;

internal static class AccountingResults
{
    /// <summary>204 on success, otherwise the usual mapping (404, 409 with the reason, 400).</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
