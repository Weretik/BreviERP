using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Media.Api.Access;

internal static class MediaAccess
{
    /// <summary>«Бібліотека фото» on «Права доступу»: Manager and Admin until someone saves other rights.</summary>
    public const string Policy = "access:media";

    /// <summary>Hard ceiling for the multipart body; the configured photo limit is checked by the upload validator.</summary>
    public const long MaxRequestBytes = 32L * 1024 * 1024;

    /// <summary>204 on success, otherwise the shared status mapping.</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
