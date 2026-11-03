using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Media.Api.Access;

internal static class MediaAccess
{
    /// <summary>Same staff as product management: Manager and Admin.</summary>
    public const string Policy = "CanManageProducts";

    /// <summary>Hard ceiling for the multipart body; the configured photo limit is checked by the upload validator.</summary>
    public const long MaxRequestBytes = 32L * 1024 * 1024;

    /// <summary>204 on success, otherwise the shared status mapping.</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
