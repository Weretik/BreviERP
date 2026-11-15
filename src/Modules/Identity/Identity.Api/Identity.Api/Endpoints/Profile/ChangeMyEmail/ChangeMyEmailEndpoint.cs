using Identity.Application.Features.Profile.Commands.ChangeOwnEmail;
using Identity.Application.Security.Policies;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.ChangeMyEmail;

internal static class ChangeMyEmailEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("email", HandleAsync).WithName("changeMyEmail")
            .WithSummary("Change own email (administrators)")
            .WithDescription("Needs the current password; a wrong one counts towards lockout. Other sessions end; this browser stays signed in.").Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PolicyNames.RequireAdminRole);

    private static async Task<IResult> HandleAsync(ChangeOwnEmailRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeOwnEmailCommand(request.Email, request.CurrentPassword), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
