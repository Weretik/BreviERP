using System.Globalization;
using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Mvc;

namespace Production.Api.Access;

internal static class ProductionAccess
{
    /// <summary>«Швейний цех»: the head of the workshop, Admin and Director.</summary>
    public const string Policy = "CanManageWorkshop";

    /// <summary>Id of the signed-in staff user, kept as «who changed it».</summary>
    public static int? UserId(this ControllerBase controller) =>
        int.TryParse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;

    /// <summary>204 on success, otherwise the shared status mapping.</summary>
    public static ActionResult ToNoContentResult(this ControllerBase controller, Result result) =>
        result.IsSuccess ? controller.NoContent() : controller.ToActionResult<Result>(result);
}
