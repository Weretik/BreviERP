using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Files;

internal static class PrivateFileHttpErrors
{
    internal static ActionResult Map<T>(ControllerBase controller, Result<T> result)
    {
        var codes = result.ValidationErrors.Select(x => x.ErrorMessage).Concat(result.Errors).ToArray();
        if (codes.Contains("private_file_storage_unavailable"))
            return controller.StatusCode(503, new { code = "private_file_storage_unavailable" });
        if (codes.Contains("file_too_large")) return controller.StatusCode(413, new { code = "file_too_large" });
        if (codes.Contains("unsupported_file_type")) return controller.StatusCode(415, new { code = "unsupported_file_type" });
        return controller.ToActionResult(result);
    }
}
