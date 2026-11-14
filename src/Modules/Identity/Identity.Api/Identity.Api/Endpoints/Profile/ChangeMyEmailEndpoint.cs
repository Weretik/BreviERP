using Identity.Application.Features.Profile.Commands.ChangeOwnEmail;
using Identity.Application.Security.Policies;

namespace Identity.Api.Endpoints.Profile;

internal static class ChangeMyEmailEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("email", HandleAsync).WithName("changeMyEmail").Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PolicyNames.RequireAdminRole);

    private static async Task<IResult> HandleAsync(ChangeOwnEmailRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeOwnEmailCommand(request.Email, request.CurrentPassword), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeOwnEmailRequest(string Email, string CurrentPassword);
