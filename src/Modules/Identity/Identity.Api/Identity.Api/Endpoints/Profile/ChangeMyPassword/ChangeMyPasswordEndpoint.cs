using Identity.Application.Features.Profile.Commands.ChangeOwnPassword;
using Identity.Application.Security.Policies;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.ChangeMyPassword;

internal static class ChangeMyPasswordEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("password", HandleAsync).WithName("changeMyPassword")
            .WithSummary("Change own password (administrators)")
            .WithDescription("Needs the current password; a wrong one counts towards lockout. Other sessions end; this browser stays signed in.").Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PolicyNames.RequireAdminRole);

    private static async Task<IResult> HandleAsync(ChangeOwnPasswordRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeOwnPasswordCommand(request.CurrentPassword, request.NewPassword), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
