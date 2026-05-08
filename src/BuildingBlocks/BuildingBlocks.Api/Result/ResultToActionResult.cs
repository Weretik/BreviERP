using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Result;

public static class ResultToActionResult
{
    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
        => result.Status switch
        {
            ResultStatus.Ok => controller.Ok(result.Value),
            ResultStatus.NotFound => controller.NotFound(),
            ResultStatus.Invalid => controller.BadRequest(result.ValidationErrors),
            ResultStatus.Conflict => controller.Conflict(result.Errors),
            ResultStatus.Forbidden => controller.Forbid(),
            ResultStatus.Unauthorized => controller.Unauthorized(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
}
