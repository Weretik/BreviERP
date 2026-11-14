using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Attachments;

internal static class FileResults
{
    /// <summary>The usual mapping, plus 503 with the reason while the file storage is not configured.</summary>
    public static ActionResult ToFileActionResult<T>(this ControllerBase controller, Result<T> result) =>
        result.Status == ResultStatus.Unavailable
            ? controller.StatusCode(StatusCodes.Status503ServiceUnavailable, result.Errors)
            : controller.ToActionResult(result);
}
