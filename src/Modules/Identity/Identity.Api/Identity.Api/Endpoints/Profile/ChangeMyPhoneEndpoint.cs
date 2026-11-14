using Identity.Application.Features.Profile.Commands.ChangeOwnPhone;
using Identity.Application.Security.Policies;

namespace Identity.Api.Endpoints.Profile;

internal static class ChangeMyPhoneEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("phone", HandleAsync).WithName("changeMyPhone").Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PolicyNames.RequireAdminRole);

    private static async Task<IResult> HandleAsync(ChangePhoneRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeOwnPhoneCommand(request.PhoneNumber), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangePhoneRequest(string? PhoneNumber);
