using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Reminders.Api.Access;

internal static class ResultNoContent
{
    /// <summary>204 on success, otherwise the shared status mapping.</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
